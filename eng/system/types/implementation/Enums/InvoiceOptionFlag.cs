using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(FlexibleEnumJsonConverter<InvoiceOptionFlag>))]
public enum InvoiceOptionFlag
{
    Generate = 0,
    Omit = 1
}
