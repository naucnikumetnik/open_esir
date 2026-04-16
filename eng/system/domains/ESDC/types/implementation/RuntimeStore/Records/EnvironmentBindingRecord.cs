using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;
using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;
using OpenFiscalCore.System.Domains.ESDC.Types.Shared;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Records;

public sealed record EnvironmentBindingRecord(
    EnvironmentBindingKey BindingKey,
    EnvironmentDescriptor CurrentEnvironment,
    EnvironmentDescriptor? LastKnownGoodEnvironment,
    StoredRecordMeta Meta);
