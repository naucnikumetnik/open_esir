namespace OpenFiscalCore.System.Qualification;

/// <summary>
/// Result of a single behavioral test case execution.
/// </summary>
internal sealed record TestResult(
    string TestCaseId,
    string Title,
    TestStatus Status,
    TimeSpan Duration,
    string? FailureMessage = null)
{
    public static TestResult Pass(string testCaseId, string title, TimeSpan duration)
        => new(testCaseId, title, TestStatus.Pass, duration);

    public static TestResult Fail(string testCaseId, string title, TimeSpan duration, string message)
        => new(testCaseId, title, TestStatus.Fail, duration, message);

    public static TestResult Skip(string testCaseId, string title, string reason)
        => new(testCaseId, title, TestStatus.Skip, TimeSpan.Zero, reason);

    public static TestResult Blocked(string testCaseId, string title, string reason)
        => new(testCaseId, title, TestStatus.Blocked, TimeSpan.Zero, reason);
}

/// <summary>
/// Outcome classification of a behavioral test case.
/// </summary>
internal enum TestStatus
{
    Pass,
    Fail,
    Skip,
    Blocked
}
