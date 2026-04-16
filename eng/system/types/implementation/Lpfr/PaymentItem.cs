using System.ComponentModel.DataAnnotations;
using OpenFiscalCore.System.Types.Enums;

namespace OpenFiscalCore.System.Types.Lpfr;

public sealed record PaymentItem(
    [property: Range(typeof(decimal), "0", "79228162514264337593543950335")]
    decimal Amount,
    TaxCorePaymentType PaymentType);
