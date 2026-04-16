using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Media;

[JsonConverter(typeof(EnumMemberJsonStringConverter<FilesystemKind>))]
public enum FilesystemKind
{
    [EnumMember(Value = "FAT32")]
    Fat32 = 0,

    [EnumMember(Value = "NTFS")]
    Ntfs = 1,

    [EnumMember(Value = "ext4")]
    Ext4 = 2,

    [EnumMember(Value = "unsupported")]
    Unsupported = 3
}
