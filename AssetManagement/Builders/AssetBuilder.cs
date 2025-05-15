using Shiftless.Clockwork.Assets.Editor.AssetManagement.Settings;
using System.IO;
using System.Text.RegularExpressions;

namespace Shiftless.Clockwork.Assets.Editor.AssetManagement.Builders
{
    public abstract class AssetBuilder
    {
        // Values
        private Regex? _regex;


        // Properties
        public abstract string Name { get; }
        public abstract string FileExtensionPattern { get; }

        public abstract string Extension { get; }


        // Func
        internal void Initialize()
        {
            _regex = new(@$"({FileExtensionPattern})$", RegexOptions.Compiled);
        }

        public string GetSourcePath(ProjectContext context) => $@"{context.SourceDirectory}\{Name.ToLower()}";
        public string GetOutputPath(ProjectContext context) => $@"{context.OutputDirectory}\{Name.ToLower()}";

        public bool HandlesExtension(string extension) => _regex?.IsMatch(extension.TrimStart('.')) ?? false;
        public bool HandlesFile(string path)
        {
            string extension = CleanExtension(Path.GetExtension(path));
            return HandlesExtension(extension);
        }
        private static string CleanExtension(string extension) => extension.Trim().TrimStart('.').ToLowerInvariant();


        // Abstract func
        public abstract (long, long) Encode(string path, string output, AssetSettings settings);
        public abstract AssetProperties GetProperties(string path);
        public abstract AssetSettings CreateSettings();
    }
}
