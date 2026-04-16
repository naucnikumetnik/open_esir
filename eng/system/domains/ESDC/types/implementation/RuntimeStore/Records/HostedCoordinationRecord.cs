using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;
using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Records;

public sealed record HostedCoordinationRecord(
    HostedTenantKey TenantKey,
    string TenantIdentifier,
    EnvironmentBindingKey? EnvironmentBindingKey,
    bool EdgeCopyOrCacheProjectionOnly,
    StoredRecordMeta Meta);
