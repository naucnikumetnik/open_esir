using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(FlexibleEnumJsonConverter<TaxCorePaymentType>))]
public enum TaxCorePaymentType
{
    Other = 0,
    Cash = 1,
    Card = 2,
    Check = 3,
    WireTransfer = 4,
    Voucher = 5,
    MobileMoney = 6
}
