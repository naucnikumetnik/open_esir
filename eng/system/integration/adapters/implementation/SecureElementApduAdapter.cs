using System.IO;
using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;
using OpenFiscalCore.System.Domains.ESDC.Types.SecureElement;
using OpenFiscalCore.System.Interfaces.External;
using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Integration.Adapters;

public sealed class SecureElementApduAdapter : ISecureElementDependency
{
    private const string DependencyName = "SecureElement";

    private readonly Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> _transmit;
    private readonly SecureElementApduAdapterConfig _config;

    public SecureElementApduAdapter(
        Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> transmit,
        SecureElementApduAdapterConfig config)
    {
        _transmit = transmit;
        _config = config;
    }

    public SignInvoiceApduResponse SignInvoiceApdu(SignInvoiceApduRequest request) =>
        ExecuteCommand(
            nameof(SignInvoiceApdu),
            SecureElementApduCodec.BuildSignInvoiceCommand(request),
            SecureElementApduCodec.ParseSignInvoiceResponse);

    public AuditRequestPayload StartAuditApdu() =>
        ExecuteCommand(
            nameof(StartAuditApdu),
            SecureElementApduCodec.BuildStartAuditCommand(),
            static payload => new AuditRequestPayload(payload.ToArray()));

    public AmountStatusResponse AmountStatusApdu() =>
        ExecuteCommand(
            nameof(AmountStatusApdu),
            SecureElementApduCodec.BuildAmountStatusCommand(),
            SecureElementApduCodec.ParseAmountStatusResponse);

    public void EndAuditApdu(ProofOfAudit proof)
    {
        _ = ExecuteCommand(
            nameof(EndAuditApdu),
            SecureElementApduCodec.BuildEndAuditCommand(proof),
            static _ => true);
    }

    public LastSignedInvoiceResponse GetLastSignedInvoiceApdu() =>
        ExecuteCommand(
            nameof(GetLastSignedInvoiceApdu),
            SecureElementApduCodec.BuildGetLastSignedInvoiceCommand(),
            SecureElementApduCodec.ParseLastSignedInvoiceResponse);

    public SecureElementVersionResponse GetSecureElementVersion() =>
        ExecuteCommand(
            nameof(GetSecureElementVersion),
            SecureElementApduCodec.BuildGetSecureElementVersionCommand(),
            SecureElementApduCodec.ParseSecureElementVersionResponse);

    public SecureElementCertParamsResponse GetCertParams() =>
        ExecuteCommand(
            nameof(GetCertParams),
            SecureElementApduCodec.BuildGetCertParamsCommand(),
            SecureElementApduCodec.ParseCertParamsResponse);

    public void ForwardSecureElementDirective(ForwardSecureElementDirectiveRequest request)
    {
        _ = ExecuteCommand(
            nameof(ForwardSecureElementDirective),
            SecureElementApduCodec.BuildForwardDirectiveCommand(request),
            static _ => true);
    }

    public PinTriesLeft GetPinTriesLeft() =>
        ExecuteCommand(
            nameof(GetPinTriesLeft),
            SecureElementApduCodec.BuildGetPinTriesLeftCommand(),
            SecureElementApduCodec.ParsePinTriesLeftResponse);

    public ExportedCertificateDer ExportCertificateApdu() =>
        ExecutePkiCommand(
            nameof(ExportCertificateApdu),
            SecureElementApduCodec.BuildExportCertificateCommand(),
            SecureElementApduCodec.ParseExportCertificateResponse);

    public TaxCorePublicKey ExportTaxCorePublicKeyApdu() =>
        ExecuteCommand(
            nameof(ExportTaxCorePublicKeyApdu),
            SecureElementApduCodec.BuildExportTaxCorePublicKeyCommand(),
            SecureElementApduCodec.ParseExportTaxCorePublicKeyResponse);

    public ExportedAuditData ExportAuditDataApdu() =>
        ExecuteCommand(
            nameof(ExportAuditDataApdu),
            SecureElementApduCodec.BuildExportAuditDataCommand(),
            SecureElementApduCodec.ParseExportAuditDataResponse);

    private T ExecuteCommand<T>(
        string operationName,
        ReadOnlyMemory<byte> command,
        Func<ReadOnlyMemory<byte>, T> parseResponse)
    {
        try
        {
            if (_config.AutoSelectApplication)
            {
                var selectCommand = SecureElementApduCodec.BuildSelectApplicationCommand(_config.SecureElementApplicationIdHex);
                _ = Transmit(selectCommand, $"{operationName}.SelectApplication");
            }

            var payload = Transmit(command, operationName);
            return parseResponse(payload);
        }
        catch (ExternalDependencyFailureException)
        {
            throw;
        }
        catch (InvalidDataException exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Protocol,
                "The secure element returned an unexpected APDU payload.",
                exception);
        }
        catch (ArgumentException exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Protocol,
                "The APDU payload could not be mapped to the canonical contract.",
                exception);
        }
        catch (FormatException exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Configuration,
                "The secure-element adapter configuration is invalid.",
                exception);
        }
        catch (OverflowException exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Protocol,
                "The APDU payload exceeded the supported canonical bounds.",
                exception);
        }
    }

    private ReadOnlyMemory<byte> Transmit(ReadOnlyMemory<byte> command, string operationName)
    {
        try
        {
            var rawResponse = _transmit(command);
            return SecureElementApduCodec.EnsureSuccessfulResponse(rawResponse, DependencyName, operationName);
        }
        catch (ExternalDependencyFailureException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Device,
                "The secure-element transport failed while transmitting an APDU command.",
                exception);
        }
    }

    private T ExecutePkiCommand<T>(
        string operationName,
        ReadOnlyMemory<byte> command,
        Func<ReadOnlyMemory<byte>, T> parseResponse)
    {
        try
        {
            var selectPkiCommand = SecureElementApduCodec.BuildSelectPkiAppletCommand(_config.PkiApplicationIdHex);
            _ = Transmit(selectPkiCommand, $"{operationName}.SelectPkiApplet");

            var payload = Transmit(command, operationName);
            return parseResponse(payload);
        }
        catch (ExternalDependencyFailureException)
        {
            throw;
        }
        catch (InvalidDataException exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Protocol,
                "The secure element returned an unexpected APDU payload.",
                exception);
        }
        catch (ArgumentException exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Protocol,
                "The APDU payload could not be mapped to the canonical contract.",
                exception);
        }
        catch (FormatException exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Configuration,
                "The secure-element adapter configuration is invalid.",
                exception);
        }
        catch (OverflowException exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Protocol,
                "The APDU payload exceeded the supported canonical bounds.",
                exception);
        }
    }
}
