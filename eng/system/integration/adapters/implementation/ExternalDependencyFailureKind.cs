namespace OpenFiscalCore.System.Integration.Adapters;

/// <summary>
/// Classifies adapter-level dependency failures after transport or technology
/// details are translated into canonical boundary errors.
/// </summary>
public enum ExternalDependencyFailureKind
{
    Configuration = 0,
    Authentication = 1,
    Transport = 2,
    Unavailable = 3,
    Protocol = 4,
    Serialization = 5,
    Device = 6,
    Certificate = 7,
    Filesystem = 8,
    NotFound = 9
}
