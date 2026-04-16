using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<RSFiscalDocumentKind>))]
public enum RSFiscalDocumentKind
{
    [EnumMember(Value = "kopija_racuna")]
    KopijaRacuna = 0,

    [EnumMember(Value = "racun_obuke")]
    RacunObuke = 1,

    [EnumMember(Value = "predracun")]
    Predracun = 2
}
