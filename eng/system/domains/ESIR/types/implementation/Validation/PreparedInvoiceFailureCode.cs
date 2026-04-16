using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESIR.Types.Validation;

[JsonConverter(typeof(EnumMemberJsonStringConverter<PreparedInvoiceFailureCode>))]
public enum PreparedInvoiceFailureCode
{
    [EnumMember(Value = "VALIDATION_FAILED")]
    ValidationFailed = 0
}
