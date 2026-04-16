using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<RSTransactionKind>))]
public enum RSTransactionKind
{
    [EnumMember(Value = "prodaja")]
    Prodaja = 0,

    [EnumMember(Value = "refundacija_ponistavanje")]
    RefundacijaPonistavanje = 1
}
