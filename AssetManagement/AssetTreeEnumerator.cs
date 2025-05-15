using System.Collections;

namespace Shiftless.Clockwork.Assets.Editor.AssetManagement
{
    internal class AssetTreeEnumerator : IEnumerator<AssetHandle>
    {
        internal readonly AssetTree Tree;

        private AssetTreeEnumeratorContext _context;

        internal AssetTreeEnumerator(AssetTree tree)
        {
            Tree = tree;
            _context = new(Tree);
        }

        AssetHandle IEnumerator<AssetHandle>.Current => _context.GetCurrent();

        object IEnumerator.Current => _context.GetCurrent();

        void IDisposable.Dispose()
        {
        }

        bool IEnumerator.MoveNext() => _context.MoveNext();

        void IEnumerator.Reset() => _context.Child = null;

        private class AssetTreeEnumeratorContext(IAssetTreeDirectory directory)
        {
            public AssetTreeEnumeratorContext? Child;

            public readonly IAssetTreeDirectory Directory = directory;
            public int Offset = -1;

            public AssetHandle GetCurrent()
            {
                if (Child != null)
                    return Child.GetCurrent();

                return ((AssetTreeFile)Directory.Children[Offset]).Handle;
            }

            public bool MoveNext()
            {
                if (Child != null)
                {
                    if (Child.MoveNext())
                        return true;

                    Child = null;
                }

                Offset++;

                if (Offset >= Directory.Children.Count)
                    return false;

                AssetTreeNode node = Directory.Children[Offset];

                if (node is IAssetTreeDirectory directory)
                {
                    Child = new(directory);
                    return MoveNext();
                }

                return true;
            }
        }
    }
}
