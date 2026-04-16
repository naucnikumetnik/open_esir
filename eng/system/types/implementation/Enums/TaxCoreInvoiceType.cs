using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(FlexibleEnumJsonConverter<TaxCoreInvoiceType>))]
public enum TaxCoreInvoiceType
{
    Normal = 0,
    ProForma = 1,
    Copy = 2,
    Training = 3,
    Advance = 4
}
