using Shiftless.Clockwork.Assets.Editor.AssetManagement;
using System.Windows;
using System.Windows.Controls;

namespace Shiftless.Clockwork.Assets.Editor
{
    /// <summary>
    /// Interaction logic for FileControl.xaml
    /// </summary>
    public partial class AssetTreeNodeControl : UserControl
    {
        public readonly AssetTreeNode Node;

        public AssetTreeNodeControl(AssetTreeNode node)
        {
            Node = node;

            InitializeComponent();

            Label.Text = Node.Name;

            if (Node.RequiresUpdate)
            {
                Label.Text = $"* {Label.Text}";
                Label.FontStyle = FontStyles.Italic;
            }
        }
    }
}
