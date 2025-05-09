using Shiftless.Clockwork.Assets.Editor.AssetManagement.Builders;
using Shiftless.Clockwork.Assets.Editor.AssetManagement.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Shiftless.Clockwork.Assets.Editor.AssetManagement
{
    public readonly struct AssetHandle
    {
        // Values
        public readonly string Name;
        public readonly string LocalDir;

        public readonly string SourcePath;

        public readonly AssetBuilder Builder;
        public readonly AssetSettings Settings;
        public readonly AssetProperties Properties;


        // Properties
        public string FullName => Path.Combine(LocalDir, Name);

        public string Extension => Path.GetExtension(SourcePath);
        public string LocalPath => AssetUtils.LocalizePath(SourcePath);

        public string OutputPath => Path.Combine(Project.OutputDirectory, $"{FullName}.{Builder.Extension}");

        public bool SourceExists => File.Exists(SourcePath);


        // Constructor
        public AssetHandle(string name, string localDir, string sourcePath, AssetBuilder builder, AssetSettings settings)
        {
            Name = name;
            LocalDir = localDir;
            SourcePath = sourcePath;
            Builder = builder;
            Settings = settings;

            Properties = builder.GetProperties(sourcePath);

            Project.Assets.AddAsset(FullName, this);
        }


        // Func
        public bool IsSourceNewer()
        {
            DateTime sourceTime = File.GetLastWriteTime(SourcePath);
            DateTime outputTime = File.GetLastWriteTime(OutputPath);

            return sourceTime > outputTime;
        }

        internal (long, long) Build() => Builder.Encode(SourcePath, OutputPath, Settings);

        internal void Dispose()
        {
            Project.Assets.RemoveAsset(FullName);
        }
    }
}
