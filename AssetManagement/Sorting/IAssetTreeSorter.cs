using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shiftless.Clockwork.Assets.Editor.AssetManagement.Sorting
{
    public interface IAssetTreeSorter
    {
        IEnumerable<AssetTreeNode> Sort(IAssetTreeDirectory directory);
    }
}
