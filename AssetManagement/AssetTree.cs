using Shiftless.Common.Serialization;
using Shiftless.Clockwork.Assets.Editor.AssetManagement.Builders;
using Shiftless.Clockwork.Assets.Editor.AssetManagement.Settings;
using Shiftless.Clockwork.Assets.Editor.AssetManagement.Sorting;
using Shiftless.Clockwork.Assets.Editor.Exceptions;
using System.Collections;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace Shiftless.Clockwork.Assets.Editor.AssetManagement
{
    public sealed class AssetTree : IAssetTreeDirectory, IEnumerable<AssetHandle>, IByteSerializable
    {
        // Values
        public readonly Project Project;

        private List<AssetTreeNode> _children = [];

        private Dictionary<string, AssetHandle> _assets = [];


        // Properties
        public IReadOnlyList<AssetTreeNode> Children => _children;
        public IEnumerable<AssetHandle> Assets => _assets.Values;

        public string SourcePath => Project.SourceDirectory;


        // Events
        public event Action? OnRefresh;


        // Constructor
        internal AssetTree(Project project)
        {
            Project = project;
        }


        // Func
        //public bool Contains(string localPath) => _assets.ContainsKey(localPath);
        public bool ManagesName(string name) => Assets.Any(asset => asset.Name == name);
        public bool ManagesSource(string path) => Assets.Any(asset => asset.SourcePath == path);
        public bool ManagesOutput(string path) => Assets.Any(asset => asset.OutputPath == path);

        public AssetHandle GetAsset(string name) => _assets[name];
        public bool TryGetAsset(string name, out AssetHandle assetHandle)
        {
            try
            {
                assetHandle = GetAsset(name);
                return true;
            }
            catch (KeyNotFoundException)
            {
                assetHandle = default;
                return false;
            }
        }

        public void Sort(IAssetTreeSorter sortMode) => _children = sortMode.Sort(this).ToList();

        internal void AddAsset(string name, AssetHandle asset) => _assets.Add(name, asset);
        internal void RemoveAsset(string name) => _assets.Remove(name);

        public void Refresh()
        {
            OnRefresh?.Invoke();

            // Remove directories that dont exist no more
            Queue<AssetTreeDirectory> removalQueue = new Queue<AssetTreeDirectory>();
            foreach(AssetTreeDirectory child in _children)
            {
                if (Directory.Exists(child.SourcePath))
                    continue;

                removalQueue.Enqueue(child);
            }
            while (removalQueue.TryDequeue(out AssetTreeDirectory? dir))
                dir.Dispose();

            // Now index new ones
            foreach (string directory in Directory.GetDirectories(SourcePath))
            {
                string localDirectory = AssetUtils.LocalizePath(directory);

                // If we exclude this directory continue
                if (AssetUtils.ExcludeDirectory(localDirectory + Path.DirectorySeparatorChar))
                    continue;

                string name = Path.GetFileName(localDirectory);

                // If we already have this child, continue. We also dont have to refresh as that already happened by the event.
                if (ContainsChild(name))
                    continue;

                if (Directory.GetFiles(directory).Length == 0)
                    continue;

                AssetTreeDirectory node = new(this, directory);
                AddChild(node);

                node.Refresh();
            }

        }

        internal TreeViewItem BuildTreeUi()
        {
            TreeViewItem item = new() { Header = "assets" };

            foreach (AssetTreeNode node in _children)
                item.Items.Add(node.ToTreeViewItem());

            return item;
        }

        private void AddChild(AssetTreeNode node)
        {
            if (node is not IAssetTreeDirectory directory)
                throw new InvalidOperationException("Somehow, god knows how, the root asset tree got a child, which is not a directory");

            _children.Add(node);
            OnRefresh += directory.Refresh;
        }
        private void RemoveChild(AssetTreeNode node)
        {
            if (node is not IAssetTreeDirectory directory)
                throw new InvalidOperationException("Somehow, god knows how, the root asset tree got a child, which is not a directory");

            _children.Remove(node);
            OnRefresh -= directory.Refresh;
        }

        IEnumerator<AssetHandle> IEnumerable<AssetHandle>.GetEnumerator() => new AssetTreeEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new AssetTreeEnumerator(this);

        IEnumerable<AssetTreeDirectory> IAssetTreeDirectory.GetDirectories() => _children.OfType<AssetTreeDirectory>();
        IEnumerable<AssetTreeFile> IAssetTreeDirectory.GetFiles() => _children.OfType<AssetTreeFile>();

        void IAssetTreeDirectory.AddChild(AssetTreeNode node) => AddChild(node);
        void IAssetTreeDirectory.RemoveChild(AssetTreeNode node) => RemoveChild(node);
        public bool ContainsChild(string name) => _children.Any(node => node.Name == name);
        AssetTreeNode? IAssetTreeDirectory.GetChild(string name) => _children.FirstOrDefault(node => node.Name == name);

        public byte[] Serialize()
        {
            ByteWriter writer = new();
            writer.Write("tree", null);

            foreach (AssetHandle handle in _assets.Values)
            {
                writer.Write(handle.LocalPath);
                writer.Write(handle.Name);
                writer.Write(handle.Settings.Serialize());
            }
            return writer.ToArray();
        }

        public void Deserialize(ByteStream stream)
        {
            if (_assets.Count != 0)
                throw new InvalidOperationException("AssetTree was already initialized!");

            string header = stream.ReadString(4);
            if (header != "tree")
                throw new InvalidDataException("Could not match AssetTree header during deserialization!");

            while (!stream.IsAtEnd)
            {
                string path = Path.Combine(Project.SourceDirectory, stream.ReadString());
                string name = stream.ReadString();

                if (!Project.Context.TryGetAssetBuilder(path, out AssetBuilder? builder))
                    throw new UnhandledFileException(path);

                AssetSettings settings = builder.CreateSettings();
                settings.Deserialize(stream);

                string localPath = AssetUtils.LocalizePath(path);

                if (!File.Exists(path))
                {
                    Project.Console.WriteLine($"File at {localPath} went missing!");
                    continue;
                }

                string localDir = AssetUtils.GetLocalDirectory(localPath);

                AssetHandle handle = new(name, localDir, path, builder, settings);
                AddAssetNodeRecursive(handle, this, handle.FullName.Split('\\'), 0);
            }
        }

        private void AddAssetNodeRecursive(AssetHandle handle, IAssetTreeDirectory parent, string[] path, int index)
        {
            if (index == path.Length - 1)
            {
                parent.AddChild(new AssetTreeFile(parent, handle));
                return;
            }

            string fullPath = Path.Combine(path[..(index + 1)]);
            AssetTreeDirectory? directory = parent.GetDirectories().FirstOrDefault(dir => dir.Name == path[index]);
            if (directory == null)
            {
                directory = new(parent, Path.Combine(Project.SourceDirectory, fullPath));
                parent.AddChild(directory);
            }

            AddAssetNodeRecursive(handle, directory, path, index + 1);
        }
    }

    public interface IAssetTreeDirectory
    {
        // Properties
        string SourcePath { get; }

        IReadOnlyList<AssetTreeNode> Children { get; }


        // Events
        event Action? OnRefresh;


        // Func
        void Refresh();
        void Sort(IAssetTreeSorter sortMode);

        bool ContainsChild(string name);
        AssetTreeNode? GetChild(string name);

        void AddChild(AssetTreeNode node);
        void RemoveChild(AssetTreeNode node);

        IEnumerable<AssetTreeDirectory> GetDirectories();
        IEnumerable<AssetTreeFile> GetFiles();
    }

    public abstract class AssetTreeNode
    {
        // Properties
        public readonly IAssetTreeDirectory Parent;

        public abstract string Name { get; }
        public abstract bool IsDirectory { get; }

        public abstract bool RequiresUpdate { get; }

        public abstract string SourcePath { get; }

        public DateTime LastWriteTime => Directory.GetLastWriteTime(SourcePath);


        // Constructor
        internal AssetTreeNode(IAssetTreeDirectory parent) => Parent = parent;


        // Abstract Func
        internal abstract TreeViewItem ToTreeViewItem();


        // Func
        public AssetTreeDirectory ToDirectory()
        {
            if (this is not AssetTreeDirectory value)
                throw new InvalidCastException("Asset tree node was not a directory!");

            return value;
        }


        // Overrides
        public string GetFullName()
        {
            if (Parent is AssetTreeNode node)
                return $"{node.Name}\\{Name}";

            return Name;
        }

        public override string ToString() => GetFullName();
    }

    public sealed class AssetTreeDirectory : AssetTreeNode, IAssetTreeDirectory
    {
        // Values
        private readonly string _directory;

        private List<AssetTreeNode> _children = [];

        private DateTime _lastRefresh;


        // Properties
        public IReadOnlyList<AssetTreeNode> Children => _children;

        public override string SourcePath => _directory;

        public override string Name => Path.GetFileName(_directory);
        public override bool IsDirectory => true;

        public override bool RequiresUpdate => HasOutdatedChildren();


        // Events
        public event Action? OnRefresh;


        // Constructor
        internal AssetTreeDirectory(IAssetTreeDirectory parent, string directory) : base(parent) => _directory = directory;


        // Func
        public void Refresh()
        {
            // First check if the directory has even updated since the last refesh
            if (LastWriteTime <= _lastRefresh)
                return;

            // Update the last refresh
            _lastRefresh = DateTime.Now;

            // All children bind to this event, so this refreshes all chidren
            OnRefresh?.Invoke();

            // First we index all the directories
            foreach (string directory in Directory.GetDirectories(SourcePath))
            {
                string localPath = AssetUtils.LocalizePath(directory);

            }


            // Now we go index the files in this directory.
            foreach (string file in Directory.GetFiles(SourcePath, "*.*"))
            {
                // First get the needed name stuff
                string localPath = AssetUtils.LocalizePath(file);
                string name = Path.GetFileNameWithoutExtension(localPath);

                // If this directory already manages this child we can safely continue
                if (ContainsChild(name))
                    continue;

                if (!Project.Context.TryGetAssetBuilder(file, out AssetBuilder? builder))
                {
                    Project.Console.WriteLine($"Encountered unhandled asset: {localPath}!");
                    continue;
                }

                AssetHandle handle = AssetUtils.CreateHandle(file);
                AddChild(new AssetTreeFile(this, handle));
            }

            // Finally check if all the files in this directory still exist
            // If remove child is called, and after this node has 0 children, it removes itself from the parent
            Queue<AssetTreeFile> removeQueue = new();
            foreach (AssetTreeFile asset in GetFiles())
            {
                if (asset.Handle.SourceExists)
                    continue;

                removeQueue.Enqueue(asset);
            }

            while (removeQueue.Count > 0)
            {
                AssetTreeFile node = removeQueue.Dequeue();
                RemoveChild(node);

                Project.Console.WriteLine($"Asset {node.Handle.LocalPath} was missing! Removing...");
            }
        }

        internal override TreeViewItem ToTreeViewItem()
        {
            TreeViewItem item = new() { Header = new AssetTreeNodeControl(this) };

            foreach (AssetTreeNode node in Children)
                item.Items.Add(node.ToTreeViewItem());

            return item;
        }

        public bool HasOutdatedChildren() => _children.Any(child => child.RequiresUpdate);

        private void AddChild(AssetTreeNode node)
        {
            _children.Add(node);

            // Check if we added a directory if so bind it from the OnRefresh event
            if (node is AssetTreeDirectory directory)
                OnRefresh += directory.OnRefresh;
        }
        private void RemoveChild(AssetTreeNode node)
        {
            // First remove the child
            _children.Remove(node);

            // Check if we removed a directory if so unbind it from the OnRefresh event
            if (node is AssetTreeDirectory directory)
                OnRefresh -= directory.OnRefresh;

            if (node is AssetTreeFile file)
                file.Handle.Dispose();

            // Now check if the children count is zero, if so we can also delete this, if we have a parent ofcourse
            if (Children.Count == 0)
                Parent.RemoveChild(this);
        }

        void IAssetTreeDirectory.AddChild(AssetTreeNode node) => AddChild(node);
        void IAssetTreeDirectory.RemoveChild(AssetTreeNode node) => RemoveChild(node);

        public IEnumerable<AssetTreeDirectory> GetDirectories() => _children.OfType<AssetTreeDirectory>();
        public IEnumerable<AssetTreeFile> GetFiles() => _children.OfType<AssetTreeFile>();

        void IAssetTreeDirectory.Sort(IAssetTreeSorter sortMode)
        {
            _children = sortMode.Sort(this).ToList();
        }
        public bool ContainsChild(string name) => _children.Any((child) => child.Name == name);
        AssetTreeNode? IAssetTreeDirectory.GetChild(string name) => _children.FirstOrDefault(child => child.Name == name);

        internal void Dispose()
        {
            foreach (AssetTreeDirectory directory in GetDirectories())
                directory.Dispose();

            while(Children.Count > 0)
                RemoveChild(Children[0]);
        }
    }

    public sealed class AssetTreeFile : AssetTreeNode
    {
        // Values
        public readonly AssetHandle Handle;


        // Properties
        public override string SourcePath => Path.GetDirectoryName(Handle.SourcePath) ?? throw new Exception("Asset directory was null?!");

        public override string Name => Handle.Name;
        public override bool IsDirectory => false;

        public override bool RequiresUpdate => Handle.IsSourceNewer();


        // Constructor
        internal AssetTreeFile(IAssetTreeDirectory parent, AssetHandle handle) : base(parent) => Handle = handle;


        // Func
        internal override TreeViewItem ToTreeViewItem() => new() { Header = new AssetTreeNodeControl(this) };
    }
}
