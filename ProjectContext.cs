using Shiftless.Clockwork.Assets.Editor.AssetManagement.Builders;
using Shiftless.Clockwork.Assets.Editor.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;

namespace Shiftless.Clockwork.Assets.Editor
{
    public sealed class ProjectContext
    {
        // Values
        public readonly Project Project;

        public readonly string SourceDirectory;
        public string OutputDirectory => $@"{SourceDirectory}\bin";

        private List<AssetBuilder> _assetBuilders = [];


        // Properties
        public IReadOnlyList<AssetBuilder> AssetBuilders => _assetBuilders;


        // Constructor
        internal ProjectContext(Project project, string source)
        {
            // First set the values
            Project = project;

            SourceDirectory = source;
        }


        // Func
        public void RegisterAssetBuilder(AssetBuilder assetHandler)
        {
            // Check if this asset handler is handlin a file extension that is already handled.
            string[] extensions = assetHandler.FileExtensionPattern.Split('|');
            foreach (string extension in extensions)
            {
                if(TryGetAssetBuilder(extension, out AssetBuilder? builder))
                    throw new InvalidOperationException($"Assets of type {extension} was already handled by {builder.GetType().Name}!");
            }

            // Add the asset handler
            _assetBuilders.Add(assetHandler);
            assetHandler.Initialize();

            // Print some cool stuff
            Project.Console.WriteLine($"Registered asset handler {assetHandler.GetType().Name}");
        }

        public bool HandlesExtension(string extension) => _assetBuilders.Any(b => b.HandlesExtension(extension));
        public AssetBuilder GetAssetBuilder(string path) => _assetBuilders.FirstOrDefault(b => b.HandlesFile(path)) ?? throw new UnhandledFileException(path);
        public bool TryGetAssetBuilder(string path, [NotNullWhen(true)] out AssetBuilder? builder)
        {
            try
            {
                builder = GetAssetBuilder(path);
                return true;

            }
            catch(UnhandledFileException)
            {
                builder = null;
                return false;
            }
        }

        public AssetBuilder GetAssetBuilderFromOutput(string path)
        {
            string ext = Path.GetExtension(path).TrimStart('.');
            return AssetBuilders.FirstOrDefault(b => b.Extension == ext) ?? throw new UnhandledFileException(path);
        }
        public bool TryGetAssetBuilderFromOutput(string extension, [NotNullWhen(true)] out AssetBuilder? handler)
        {
            handler = GetAssetBuilderFromOutput(extension);
            return handler != null;
        }
    }
}
