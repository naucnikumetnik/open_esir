using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Media;

[JsonConverter(typeof(EnumMemberJsonStringConverter<MediaDetectionFailureReason>))]
public enum MediaDetectionFailureReason
{
    [EnumMember(Value = "unsupported_filesystem")]
    UnsupportedFilesystem = 0,

    [EnumMember(Value = "read_only")]
    ReadOnly = 1
}
