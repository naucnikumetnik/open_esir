using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;
using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;
using OpenFiscalCore.System.Types.Domain;
using OpenFiscalCore.System.Types.Enums;
using OpenFiscalCore.System.Types.Lpfr;
using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Records;

public sealed record LocalJournalRecord(
    LocalJournalKey JournalKey,
    Uid Uid,
    InvoiceResult InvoiceResult,
    string JournalText,
    BuyerIdentification? BuyerIdentification,
    string? BuyerCostCenterId,
    TaxCoreTransactionType? TransactionType,
    TaxCorePaymentType? PaymentType,
    TaxCoreInvoiceType? InvoiceType,
    string? InvoiceItemsData,
    StoredRecordMeta Meta);
