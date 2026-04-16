namespace OpenFiscalCore.System.Qualification;

/// <summary>
/// Formats and prints behavioral test results to the console.
/// </summary>
internal static class TestReporter
{
    public static void PrintSection(string sectionName)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {sectionName} ===");
    }

    public static void PrintResults(IReadOnlyList<TestResult> results)
    {
        foreach (var result in results)
        {
            var tag = result.Status switch
            {
                TestStatus.Pass => "[PASS]",
                TestStatus.Fail => "[FAIL]",
                TestStatus.Skip => "[SKIP]",
                TestStatus.Blocked => "[BLKD]",
                _ => "[????]"
            };

            var duration = result.Duration.TotalMilliseconds > 0
                ? $" ({result.Duration.TotalMilliseconds:F0}ms)"
                : "";

            Console.Write($"{tag} {result.TestCaseId} — {result.Title}{duration}");

            if (result.Status == TestStatus.Fail && result.FailureMessage is not null)
                Console.Write($": {result.FailureMessage}");
            else if (result.Status == TestStatus.Skip && result.FailureMessage is not null)
                Console.Write($": {result.FailureMessage}");
            else if (result.Status == TestStatus.Blocked && result.FailureMessage is not null)
                Console.Write($": {result.FailureMessage}");

            Console.WriteLine();
        }
    }

    public static void PrintSummary(IReadOnlyList<TestResult> allResults)
    {
        var passed = allResults.Count(r => r.Status == TestStatus.Pass);
        var failed = allResults.Count(r => r.Status == TestStatus.Fail);
        var skipped = allResults.Count(r => r.Status == TestStatus.Skip);
        var blocked = allResults.Count(r => r.Status == TestStatus.Blocked);
        var totalMs = allResults.Sum(r => r.Duration.TotalMilliseconds);

        Console.WriteLine();
        Console.Write($"Summary: {passed} passed");
        if (failed > 0) Console.Write($", {failed} failed");
        if (skipped > 0) Console.Write($", {skipped} skipped");
        if (blocked > 0) Console.Write($", {blocked} blocked");
        Console.WriteLine($" ({totalMs:F0}ms)");
    }
}
