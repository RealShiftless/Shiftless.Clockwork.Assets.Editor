using Shiftless.Clockwork.Assets.Editor.AssetManagement;
using Shiftless.Clockwork.Assets.Editor.AssetManagement.Builders;
using Shiftless.Clockwork.Assets.Editor.UserControls;
using Shiftless.Common.Serialization;
using System.Diagnostics;
using System.IO;

namespace Shiftless.Clockwork.Assets.Editor
{
    public sealed class Project
    {
        // Singleton
        private static Project? _instance = null;
        public static Project Instance => _instance ?? throw new InvalidOperationException("No project was loaded!");


        // Values
        private static FileConsole? _console;

        private readonly ProjectContext _context;
        private readonly AssetTree _assets;


        // Properties
        public static FileConsole Console => _console ?? throw new InvalidOperationException("Project had no bound console!");

        public static ProjectContext Context => Instance._context;
        public static AssetTree Assets => Instance._assets;

        public static string SourceDirectory => Context.SourceDirectory;
        public static string OutputDirectory => Context.OutputDirectory;

        public static string SaveFilePath => Path.Combine(SourceDirectory, "assets.cwat");

        public static bool IsActive => _instance != null;


        // Constructor
        internal Project(FileConsole console, string directory)
        {
            // Error check
            if (_instance != null)
                throw new InvalidOperationException("A project was already loaded?!");

            // Set the values
            _instance = this;

            _console = console;
            _context = new(this, directory);
            _assets = new(this);

            // Add default asset handlers
            _context.RegisterAssetBuilder(new Texture2DBuilder());
            _context.RegisterAssetBuilder(new AudioBuilder());

            // Load if there is already a valid file
            if (File.Exists(SaveFilePath))
                _assets.Deserialize(new ByteStream(SaveFilePath));

            // And finally refresh the assets
            _assets.Refresh();
        }


        // Func
        internal void Build()
        {
            Console.WriteLine("Building project...");

            Stopwatch sw = Stopwatch.StartNew();
            foreach (AssetHandle asset in _assets)
            {
                if (!asset.IsSourceNewer())
                {
                    Console.WriteLine($"Skipping asset: {asset.LocalPath}");
                    continue;
                }

                (long sourceBytes, long outputBytes) = asset.Build();
                double sourceKb = sourceBytes / 1024f;
                double outputKb = outputBytes / 1024f;

                string sourceSize = sourceKb > 1 ? $"{sourceKb:F2}kb" : $"{sourceBytes}b";
                string outputSize = outputKb > 1 ? $"{outputKb:F2}kb" : $"{outputBytes}b";

                double comprPercentage = 100d / sourceBytes * outputBytes;

                Console.WriteLine($"Built asset: {asset.LocalPath} (s: {sourceSize}, o: {outputSize}, {comprPercentage:F2}%)");
            }
            sw.Stop();

            Console.WriteLine($"Build finished in {sw.ElapsedMilliseconds}ms!");
        }

        internal void Clean()
        {
            Console.WriteLine("Cleaning project...");

            Stopwatch sw = Stopwatch.StartNew();

            // So first we get all the files and see if they are also in the source...
            string[] files = Directory.GetFiles(_context.OutputDirectory, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if (_assets.ManagesOutput(file))
                    continue;

                Console.WriteLine($"Deleting file at {file}...");
                File.Delete(file);
            }

            // Now we get all directories and check if they're empty
            string[] directories = Directory.GetDirectories(_context.OutputDirectory, "*", SearchOption.AllDirectories);

            foreach (string directory in directories)
            {
                if (!Directory.Exists(directory))
                    continue;

                if (Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Length > 0)
                    continue;

                Directory.Delete(directory, true);
            }

            sw.Stop();

            Console.WriteLine($"Finished cleaning project in {sw.ElapsedMilliseconds}ms!");
        }


        public void Save()
        {
            File.WriteAllBytes(SaveFilePath, _assets.Serialize());
        }

        internal void Dispose()
        {
            _instance = null;
        }
    }
}
