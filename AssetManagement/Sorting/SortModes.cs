using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shiftless.Clockwork.Assets.Editor.AssetManagement.Sorting
{
    public static class SortModes
    {
        public static readonly AlphabeticTreeSorter Alphabetic;

        static SortModes()
        {
            Alphabetic = new AlphabeticTreeSorter();
        }
    }
}
