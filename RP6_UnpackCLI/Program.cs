using RP6;
using RP6.IO;

namespace RP6_UnpackCLI;

abstract class Program
{
    private static int Main(string[] args)
    {
        var iniPath = Path.Combine(AppContext.BaseDirectory, "options.ini");
        Options.LoadOrCreate(iniPath);

        if (args.Length is < 1 or > 2)
        {
            Console.WriteLine("Usage: {0} input_file.rpack [output_dir]", AppDomain.CurrentDomain.FriendlyName);
            return 1;
        }

        var inputFile = args[0];
        var outputRoot = args.Length == 2 ? args[1] : Path.GetFileNameWithoutExtension(inputFile) ?? ".";

        if (!File.Exists(inputFile))
        {
            Console.Error.WriteLine($"[ERROR] Input file does not exist: {inputFile}");
            return 1;
        }

        try
        {
            Directory.CreateDirectory(outputRoot);

            using var fs = File.OpenRead(inputFile);
            var processor = new Rp6Processor(fs);

            List<ResourceInfo> resources;
            try
            {
                resources = processor.Process(outputRoot);
            }
            finally
            {
                // your Rp6Processor has a Dispose() method; ensure it's called
                processor.Dispose();
            }

            foreach (var res in resources)
            {
                ResourceWriter.WriteResource(res);
            }

            Console.WriteLine("Done.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR] Unhandled exception: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 2;
        }
    }
}