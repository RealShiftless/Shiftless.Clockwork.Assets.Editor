using Shiftless.Clockwork.Assets.Editor.AssetManagement.Builders;
using Shiftless.Clockwork.Assets.Editor.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Shiftless.Clockwork.Assets.Editor.AssetManagement
{
    public static class AssetUtils
    {
        // Values
        private static Regex _exclusionRegex;


        // Constructor
        static AssetUtils()
        {
            _exclusionRegex = new(@"^(bin\\|_exclude\\)|.*\\\\_exclude\\.*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }


        // Func
        public static AssetHandle CreateHandle(string sourcePath)
        {
            // Error checks
            if (!sourcePath.StartsWith(Project.SourceDirectory) || sourcePath.StartsWith(Project.OutputDirectory))
                throw new Exception($"Asset at {sourcePath} was not in a valid directory!");

            if (Project.Assets.ManagesSource(sourcePath))
                throw new InvalidOperationException($"Asset at {sourcePath} was already managed!");

            // Get the asset builder, if there is none throw an error
            if (!Project.Context.TryGetAssetBuilder(sourcePath, out AssetBuilder? builder))
                throw new UnhandledFileException(sourcePath);

            // Create some variables we need
            string localDir = LocalizePath(Path.GetDirectoryName(sourcePath) ?? throw new Exception("Path is null?!?!"));
            string name = Path.GetFileNameWithoutExtension(sourcePath);

            string fullName = string.IsNullOrEmpty(localDir) ? name : Path.Combine(localDir, name);

            // Loop over the name until the name does not exist
            int i = 0;
            while (Project.Assets.ManagesName(fullName + (i != 0 ? $"_{i}" : string.Empty)))
                i++;

            // Now create the actual names
            name += i != 0 ? $"_{i}" : string.Empty;
            fullName += i != 0 ? $"_{i}" : string.Empty;

            // Create  and return the actual handle
            return new(name, localDir, sourcePath, builder, builder.CreateSettings()); ;
        }

        public static string LocalizePath(string path)
        {
            if (!path.StartsWith(Project.SourceDirectory))
                throw new ArgumentException("Path does not start with source directory!");

            return path[Project.SourceDirectory.Length..].TrimStart(Path.DirectorySeparatorChar);
        }

        public static string LocalizeOutputPath(string path)
        {
            if (!path.StartsWith(Project.SourceDirectory))
                throw new ArgumentException("Path does not start with source directory!");

            return path[Project.OutputDirectory.Length..].TrimStart(Path.DirectorySeparatorChar);
        }
        public static string GetLocalDirectory(string localPath)
        {
            string name = Path.GetFileName(localPath);
            return localPath[..^(name.Length + 1)];
        }

        public static bool ExcludeDirectory(string path)
        {
            if(!path.EndsWith(Path.DirectorySeparatorChar))
                path += Path.DirectorySeparatorChar;

            return _exclusionRegex.IsMatch(path);
        }

        public static string GetFullPath(string local)
        {
            if (string.IsNullOrEmpty(local))
                throw new ArgumentException("Local path cannot be null or empty.", nameof(local));

            if (!local.StartsWith(Path.DirectorySeparatorChar))
                local = Path.DirectorySeparatorChar + local;

            return Project.SourceDirectory + local;
        }

        public static string GetOutputName(string localName)
        {
            if (string.IsNullOrEmpty(localName))
                throw new ArgumentException("Local path cannot be null or empty.", nameof(localName));

            string name = Path.GetFileNameWithoutExtension(localName);
            string path = Path.GetDirectoryName(localName) ?? string.Empty;

            return Path.Combine(path, name);
        }

        public static string GetOutputPath(string localName, AssetBuilder builder)
        {
            string localPath = $"{GetOutputName(localName)}.{builder.Extension}";

            return Path.Combine(Project.OutputDirectory, localPath);
        }
    }
}
