using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(FlexibleEnumJsonConverter<RSBuyerIdentificationCode>))]
public enum RSBuyerIdentificationCode
{
    DomesticPib = 10,
    DomesticJmbg = 11,
    DomesticPibAndJbkjs = 12,
    PensionCard = 13,
    AgriculturalPib = 14,
    AgriculturalJmbg = 15,
    Bpg = 16,
    NationalIdCard = 20,
    RefugeeCard = 21,
    Ebs = 22,
    DomesticPassport = 23,
    ForeignPassport = 30,
    DiplomaticIdentity = 31,
    NorthMacedoniaIdCard = 32,
    MontenegroIdCard = 33,
    AlbaniaIdCard = 34,
    BosniaAndHerzegovinaIdCard = 35,
    EuOrChOrNoOrIsIdCard = 36,
    ForeignTin = 40
}
