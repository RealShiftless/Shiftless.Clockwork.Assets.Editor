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
