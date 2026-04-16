using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(FlexibleEnumJsonConverter<TaxCoreTransactionType>))]
public enum TaxCoreTransactionType
{
    Sale = 0,
    Refund = 1
}
