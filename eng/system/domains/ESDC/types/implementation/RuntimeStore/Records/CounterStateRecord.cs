using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;
using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Records;

public sealed record CounterStateRecord(
    CounterScopeKey ScopeKey,
    string EnvironmentName,
    long InvoiceSequence,
    long TransactionSequence,
    DateTimeOffset LastUpdatedUtc,
    StoredRecordMeta Meta);
