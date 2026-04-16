namespace OpenFiscalCore.System.Qualification;

internal static class Program
{
    private static int Main(string[] args)
    {
        string? wave = null;
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--wave")
            {
                wave = args[i + 1];
                break;
            }
        }

        WaveFilter.Configure(wave);

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
