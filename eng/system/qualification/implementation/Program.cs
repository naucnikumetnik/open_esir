namespace OpenFiscalCore.System.Qualification;

internal static class Program
{
    private static int Main()
    {
        try
        {
            QualificationRunner.Run();
            Console.WriteLine("Qualification checks passed.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
