using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;
using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;
using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;
using OpenFiscalCore.System.Domains.ESDC.Types.Shared;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Records;

public sealed record SharedTaxCoreSnapshotRecord(
    SharedSnapshotKey SnapshotKey,
    EnvironmentDescriptorList Environments,
    TaxCoreConfigurationResponse Configuration,
    TaxRatesResponse TaxRates,
    EncryptionCertificateBase64 EncryptionCertificate,
    StoredRecordMeta Meta);
