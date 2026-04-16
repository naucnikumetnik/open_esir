using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(FlexibleEnumJsonConverter<RSBuyerOptionalFieldCode>))]
public enum RSBuyerOptionalFieldCode
{
    Snpdv = 20,
    Lnpdv = 21,
    PpoPdv = 30,
    ZppoPdv = 31,
    MppoPdv = 32,
    IppoPdv = 33,
    CorporateCardNumber = 50,
    CorporateCardRefundPeriod = 60
}
