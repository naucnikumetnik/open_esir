using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Shared;

public sealed record EnvironmentDescriptorList(
    [property: MinLength(1)] IReadOnlyList<EnvironmentDescriptor> Items);
