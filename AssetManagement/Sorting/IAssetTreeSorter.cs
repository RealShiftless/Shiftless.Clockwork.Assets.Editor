namespace Shiftless.Clockwork.Assets.Editor.AssetManagement.Sorting
{
    public interface IAssetTreeSorter
    {
        IEnumerable<AssetTreeNode> Sort(IAssetTreeDirectory directory);
    }
}
