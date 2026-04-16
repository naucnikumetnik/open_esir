using System.Buffers.Binary;
using System.IO;
using System.Text;
using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;
using OpenFiscalCore.System.Domains.ESDC.Types.SecureElement;
using OpenFiscalCore.System.Types.Enums;

namespace OpenFiscalCore.System.Integration.Adapters;

internal static class SecureElementApduCodec
{
    internal static ReadOnlyMemory<byte> EnsureSuccessfulResponse(
        ReadOnlyMemory<byte> rawResponse,
        string dependencyName,
        string operationName)
    {
        if (rawResponse.Length < 2)
        {
            throw new ExternalDependencyFailureException(
                dependencyName,
                operationName,
                ExternalDependencyFailureKind.Protocol,
                "The secure element returned an incomplete APDU response.");
        }

        var responseSpan = rawResponse.Span;
        var sw1 = responseSpan[^2];
        var sw2 = responseSpan[^1];

        if (sw1 != 0x90)
        {
            throw new ExternalDependencyFailureException(
                dependencyName,
                operationName,
                ExternalDependencyFailureKind.Protocol,
                $"The secure element returned status words {sw1:X2}{sw2:X2}.");
        }

        return rawResponse[..^2];
    }

    internal static byte[] BuildSelectApplicationCommand(string applicationIdHex)
    {
        var aid = Convert.FromHexString(applicationIdHex);
        return BuildShortCommand(0x00, 0xA4, 0x04, 0x00, aid, le: 0);
    }

    internal static byte[] BuildSignInvoiceCommand(SignInvoiceApduRequest request)
    {
        if (request.NumberOfTaxCategories != request.TaxCategories.Count)
        {
            throw new ArgumentException("The declared tax-category count must match the provided category collection.", nameof(request));
        }

        if (request.TaxCategories.Count == 0)
        {
            throw new ArgumentException("At least one tax category must be provided.", nameof(request));
        }

        var payload = new byte[58 + (request.TaxCategories.Count * 8) + (request.Crc.HasValue ? 4 : 0)];
        var offset = 0;

        BinaryPrimitives.WriteUInt64BigEndian(payload.AsSpan(offset, 8), checked((ulong)request.DateTimeUtc.ToUniversalTime().ToUnixTimeMilliseconds()));
        offset += 8;

        WriteLeftPaddedAscii(payload.AsSpan(offset, 20), request.TaxpayerId);
        offset += 20;

        WriteLeftPaddedAscii(payload.AsSpan(offset, 20), request.BuyerId);
        offset += 20;

        payload[offset++] = checked((byte)request.InvoiceType);
        payload[offset++] = checked((byte)request.TransactionType);

        WriteUInt56BigEndian(payload.AsSpan(offset, 7), request.InvoiceAmount);
        offset += 7;

        payload[offset++] = request.NumberOfTaxCategories;

        foreach (var category in request.TaxCategories)
        {
            payload[offset++] = category.TaxCategoryId;
            WriteUInt56BigEndian(payload.AsSpan(offset, 7), category.TaxCategoryAmount);
            offset += 7;
        }

        if (request.Crc.HasValue)
        {
            BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(offset, 4), request.Crc.Value);
        }

        return BuildExtendedCommand(0x88, 0x13, 0x04, 0x00, payload, includeLe: true, le: 0);
    }

    internal static byte[] BuildStartAuditCommand() =>
        BuildExtendedCommand(0x88, 0x21, 0x04, 0x00, ReadOnlySpan<byte>.Empty, includeLe: true, le: 264);

    internal static byte[] BuildAmountStatusCommand() =>
        BuildShortCommand(0x88, 0x14, 0x04, 0x00, ReadOnlySpan<byte>.Empty, le: 14);

    internal static byte[] BuildEndAuditCommand(ProofOfAudit proof) =>
        proof.Value.Length == 256
            ? BuildExtendedCommand(0x88, 0x20, 0x04, 0x00, proof.Value.Span, includeLe: false, le: null)
            : throw new ArgumentException("ProofOfAudit must contain exactly 256 bytes.", nameof(proof));

    internal static SignInvoiceApduResponse ParseSignInvoiceResponse(ReadOnlyMemory<byte> payload)
    {
        var responseSpan = payload.Span;
        var (encryptedInternalDataLength, crcLength) = ResolveSignInvoiceLayout(responseSpan.Length);

        var dateTimeUtc = DateTimeOffset.FromUnixTimeMilliseconds((long)BinaryPrimitives.ReadUInt64BigEndian(responseSpan[..8]));
        var taxpayerId = ReadLeftPaddedAscii(responseSpan.Slice(8, 20));
        var buyerId = ReadLeftPaddedAscii(responseSpan.Slice(28, 20));
        var invoiceType = ParseEnum<TaxCoreInvoiceType>(responseSpan[48], nameof(TaxCoreInvoiceType));
        var transactionType = ParseEnum<TaxCoreTransactionType>(responseSpan[49], nameof(TaxCoreTransactionType));
        var invoiceAmount = ReadUInt56BigEndian(responseSpan.Slice(50, 7));
        var saleOrRefundCounterValue = BinaryPrimitives.ReadUInt32BigEndian(responseSpan.Slice(57, 4));
        var totalCounterValue = BinaryPrimitives.ReadUInt32BigEndian(responseSpan.Slice(61, 4));
        var encryptedInternalDataOffset = 65;
        var encryptedInternalData = responseSpan.Slice(encryptedInternalDataOffset, encryptedInternalDataLength).ToArray();
        var digitalSignatureOffset = encryptedInternalDataOffset + encryptedInternalDataLength;
        var digitalSignature = responseSpan.Slice(digitalSignatureOffset, 256).ToArray();

        uint? crc = null;
        if (crcLength == 4)
        {
            crc = BinaryPrimitives.ReadUInt32BigEndian(responseSpan.Slice(digitalSignatureOffset + 256, 4));
        }

        return new SignInvoiceApduResponse(
            dateTimeUtc,
            taxpayerId,
            buyerId,
            invoiceType,
            transactionType,
            invoiceAmount,
            saleOrRefundCounterValue,
            totalCounterValue,
            encryptedInternalData,
            digitalSignature,
            crc);
    }

    internal static AmountStatusResponse ParseAmountStatusResponse(ReadOnlyMemory<byte> payload)
    {
        if (payload.Length != 14)
        {
            throw new InvalidDataException("Amount Status APDU must return exactly 14 bytes.");
        }

        var responseSpan = payload.Span;
        return new AmountStatusResponse(
            ReadUInt56BigEndian(responseSpan[..7]),
            ReadUInt56BigEndian(responseSpan.Slice(7, 7)));
    }

    private static (int EncryptedInternalDataLength, int CrcLength) ResolveSignInvoiceLayout(int length)
    {
        foreach (var encryptedInternalDataLength in new[] { 256, 512 })
        {
            foreach (var crcLength in new[] { 0, 4 })
            {
                if (length == 65 + encryptedInternalDataLength + 256 + crcLength)
                {
                    return (encryptedInternalDataLength, crcLength);
                }
            }
        }

        throw new InvalidDataException("Unexpected Sign Invoice APDU response length.");
    }

    private static byte[] BuildShortCommand(
        byte cla,
        byte ins,
        byte p1,
        byte p2,
        ReadOnlySpan<byte> data,
        int? le)
    {
        var result = new byte[5 + data.Length + (le.HasValue ? 1 : 0)];
        result[0] = cla;
        result[1] = ins;
        result[2] = p1;
        result[3] = p2;
        result[4] = checked((byte)data.Length);

        if (!data.IsEmpty)
        {
            data.CopyTo(result.AsSpan(5, data.Length));
        }

        if (le.HasValue)
        {
            result[^1] = checked((byte)le.Value);
        }

        return result;
    }

    private static byte[] BuildExtendedCommand(
        byte cla,
        byte ins,
        byte p1,
        byte p2,
        ReadOnlySpan<byte> data,
        bool includeLe,
        int? le)
    {
        var result = new byte[7 + data.Length + (includeLe ? 2 : 0)];
        result[0] = cla;
        result[1] = ins;
        result[2] = p1;
        result[3] = p2;
        result[4] = 0x00;
        BinaryPrimitives.WriteUInt16BigEndian(result.AsSpan(5, 2), checked((ushort)data.Length));

        if (!data.IsEmpty)
        {
            data.CopyTo(result.AsSpan(7, data.Length));
        }

        if (includeLe)
        {
            BinaryPrimitives.WriteUInt16BigEndian(result.AsSpan(7 + data.Length, 2), checked((ushort)(le ?? 0)));
        }

        return result;
    }

    private static void WriteLeftPaddedAscii(Span<byte> destination, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        if (bytes.Length > destination.Length)
        {
            throw new ArgumentException($"Value '{value}' exceeds the fixed {destination.Length}-byte APDU field length.");
        }

        destination.Clear();
        bytes.CopyTo(destination[(destination.Length - bytes.Length)..]);
    }

    private static string ReadLeftPaddedAscii(ReadOnlySpan<byte> source)
    {
        var firstNonZeroIndex = 0;
        while (firstNonZeroIndex < source.Length && source[firstNonZeroIndex] == 0x00)
        {
            firstNonZeroIndex++;
        }

        return firstNonZeroIndex >= source.Length
            ? string.Empty
            : Encoding.ASCII.GetString(source[firstNonZeroIndex..]).TrimEnd('\0');
    }

    private static void WriteUInt56BigEndian(Span<byte> destination, ulong value)
    {
        if (destination.Length != 7)
        {
            throw new ArgumentException("A UInt56 field must be encoded into exactly seven bytes.", nameof(destination));
        }

        if (value > 0x00FF_FFFF_FFFF_FFFFUL)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "A UInt56 field cannot exceed 56 bits.");
        }

        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
        buffer[1..].CopyTo(destination);
    }

    private static ulong ReadUInt56BigEndian(ReadOnlySpan<byte> source)
    {
        if (source.Length != 7)
        {
            throw new ArgumentException("A UInt56 field must be decoded from exactly seven bytes.", nameof(source));
        }

        Span<byte> buffer = stackalloc byte[8];
        buffer[0] = 0x00;
        source.CopyTo(buffer[1..]);
        return BinaryPrimitives.ReadUInt64BigEndian(buffer);
    }

    private static TEnum ParseEnum<TEnum>(byte rawValue, string enumName)
        where TEnum : struct, Enum
    {
        var candidate = (TEnum)Enum.ToObject(typeof(TEnum), rawValue);
        if (!Enum.IsDefined(candidate))
        {
            throw new InvalidDataException($"Value {rawValue} is not a valid {enumName}.");
        }

        return candidate;
    }
}
