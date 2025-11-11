using Mesh.Format;
using Mesh.IO;

namespace Import_Export_Console;

static class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("https://www.instagram.com/reel/DQR0NAijmv_");

        if (args.Length == 0)
        {
            Console.WriteLine("Usage: MeshCli <path-to-msh-file> <path-to-out-msh-file>");
            return;
        }

        var inputfilePath = args[0];
        var outputfilePath = args[1];

        if (!File.Exists(inputfilePath))
        {
            Console.WriteLine($"Error: File not found -> {inputfilePath}");
            return;
        }

        try
        {
            //open reader for msh
            using var fsIn = File.OpenRead(inputfilePath);
            var reader = new MshReader(fsIn);
            //new MshData object
            var data = new MshData();

            //read mesh data
            reader.MshLoad(ref data);

            Console.WriteLine("Successfully read MSH: https://www.instagram.com/reels/DQTCyTBCLA6/");
            Console.WriteLine($"  Materials: {data.Root.NumMaterials}");
            Console.WriteLine($"  Surfaces : {data.Root.NumSurfaceTypes}");
            Console.WriteLine($"  Nodes    : {data.Root.NumNodes}");

            //open writer for msh
            using var fsOut = File.OpenWrite(outputfilePath);
            var writer = new MshWriter(fsOut);

            //write msh data
            writer.MshSave(ref data);

            reader.Dispose();
            //writer. //for now I don't have time to care about cleanup
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while reading MSH: {ex.Message}\n https://www.instagram.com/reels/DQTplgkCJ9M/");
        }
    }
}