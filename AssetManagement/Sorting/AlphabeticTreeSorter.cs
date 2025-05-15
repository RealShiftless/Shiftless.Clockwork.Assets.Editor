namespace Shiftless.Clockwork.Assets.Editor.AssetManagement.Sorting
{
    public sealed class AlphabeticTreeSorter : IAssetTreeSorter
    {
        IEnumerable<AssetTreeNode> IAssetTreeSorter.Sort(IAssetTreeDirectory directory)
        {
            foreach (IAssetTreeDirectory subDirectory in directory.GetDirectories())
                subDirectory.Sort(this);

            return directory.Children
                .OrderByDescending(n => n.IsDirectory) // Directories first (true > false)
                .ThenBy(n => n.Name, StringComparer.OrdinalIgnoreCase).AsEnumerable(); // Then alphabetical
        }
    }
}
