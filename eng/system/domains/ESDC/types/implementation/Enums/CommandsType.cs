using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Enums;

[JsonConverter(typeof(FlexibleEnumJsonConverter<CommandsType>))]
public enum CommandsType
{
    SetTaxRates = 0,
    SetTimeServerUrl = 1,
    SetVerificationUrl = 2,
    TaxCorePublicKey = 4,
    ForwardProofOfAudit = 5,
    SetTaxCoreConfiguration = 7,
    ForwardSecureElementDirective = 8
}
