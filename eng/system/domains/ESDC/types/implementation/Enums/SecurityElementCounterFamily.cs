using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<SecurityElementCounterFamily>))]
public enum SecurityElementCounterFamily
{
    [EnumMember(Value = "invoice_sequence")]
    InvoiceSequence = 0,

    [EnumMember(Value = "invoice_kind_transaction_sequence")]
    InvoiceKindTransactionSequence = 1,

    [EnumMember(Value = "tax_rate_totals")]
    TaxRateTotals = 2,

    [EnumMember(Value = "total_turnover_tax_totals")]
    TotalTurnoverTaxTotals = 3
}
