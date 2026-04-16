using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(FlexibleEnumJsonConverter<TaxCategoryType>))]
public enum TaxCategoryType
{
    TaxOnNet = 0,
    TaxOnTotal = 1,
    AmountPerQuantity = 2
}
