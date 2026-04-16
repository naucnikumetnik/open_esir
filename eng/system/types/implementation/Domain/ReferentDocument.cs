using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Types.Domain;

public sealed record ReferentDocument(
    [property: Required, MinLength(1)] string Number,
    DateTimeOffset? DocumentDateTime);
