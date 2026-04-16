using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<RSPaymentMethod>))]
public enum RSPaymentMethod
{
    [EnumMember(Value = "gotovina")]
    Gotovina = 0,

    [EnumMember(Value = "instant_placanje")]
    InstantPlacanje = 1,

    [EnumMember(Value = "platna_kartica")]
    PlatnaKartica = 2,

    [EnumMember(Value = "cek")]
    Cek = 3,

    [EnumMember(Value = "prenos_na_racun")]
    PrenosNaRacun = 4,

    [EnumMember(Value = "vaucer")]
    Vaucer = 5,

    [EnumMember(Value = "drugo_bezgotovinsko_placanje")]
    DrugoBezgotovinskoPlacanje = 6
}
