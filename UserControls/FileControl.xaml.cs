using Shiftless.Clockwork.Assets.Editor.AssetManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            if(Node.RequiresUpdate)
            {
                Label.Text = $"* {Label.Text}";
                Label.FontStyle = FontStyles.Italic;
            }
        }
    }
}
