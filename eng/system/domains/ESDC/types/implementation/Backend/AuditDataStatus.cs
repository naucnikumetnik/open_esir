namespace OpenFiscalCore.System.Domains.ESDC.Types.Backend;

public sealed record AuditDataStatus(
    AuditDataStatusCode? Status,
    IReadOnlyList<Command>? Commands)
{
    public bool IsRetryable => Status?.IsRetryable == true;

    public bool ShouldDeleteLocal => Status?.ShouldDeleteLocal == true;

    public bool ShouldHoldLocal => Status is null || Status.Value.ShouldHoldLocal;
}
