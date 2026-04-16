using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;
using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;
using OpenFiscalCore.System.Domains.ESDC.Types.Shared;
using OpenFiscalCore.System.Types.Enums;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Records;

public sealed record ApprovedConfigurationRecord(
    ConfigurationScopeKey ScopeKey,
    TaxCoreConfigurationResponse Configuration,
    SdcMode ConfiguredSdcMode,
    string ApprovalSource,
    IReadOnlyList<string>? AppliedCommandIds,
    StoredRecordMeta Meta);
