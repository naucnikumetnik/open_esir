namespace OpenFiscalCore.System.Qualification;

/// <summary>
/// Priority classification for system qualification test cases.
/// </summary>
internal enum TestPriority
{
    /// <summary>Smoke: one happy-path test per feature group.</summary>
    P1,
    /// <summary>Core regression: major alternate and negative paths.</summary>
    P2,
    /// <summary>Extended: boundary conditions, state transitions, edge cases.</summary>
    P3,
    /// <summary>Exotic: rare combinations, low regression value.</summary>
    P4
}

/// <summary>
/// Execution wave filter. Determines which priorities run in a given wave.
/// </summary>
internal static class WaveFilter
{
    private static TestPriority _maxPriority = TestPriority.P2; // Default: regression

    /// <summary>
    /// Configure the wave from a command-line argument value.
    /// </summary>
    public static void Configure(string? waveName)
    {
        _maxPriority = (waveName?.ToLowerInvariant()) switch
        {
            "smoke" => TestPriority.P1,
            "regression" => TestPriority.P2,
            "extended" => TestPriority.P3,
            "all" => TestPriority.P4,
            _ => TestPriority.P2
        };
    }

    /// <summary>
    /// Returns true if the given priority is included in the current wave.
    /// </summary>
    public static bool Includes(TestPriority priority) => priority <= _maxPriority;
}
