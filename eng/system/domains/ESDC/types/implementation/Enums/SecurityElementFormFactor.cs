using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<SecurityElementFormFactor>))]
public enum SecurityElementFormFactor
{
    [EnumMember(Value = "smart_card")]
    SmartCard = 0,

    [EnumMember(Value = "smart_sd_card")]
    SmartSdCard = 1,

    [EnumMember(Value = "usb_token")]
    UsbToken = 2,

    [EnumMember(Value = "pfx_file")]
    PfxFile = 3
}
