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

namespace Shiftless.Clockwork.Assets.Editor.UserControls.Settings
{
    /// <summary>
    /// Interaction logic for BoolControl.xaml
    /// </summary>
    public partial class BoolControl : UserControl, ISettingControl
    {
        public event Action<object>? ValueChanged;

        public BoolControl()
        {
            InitializeComponent();
        }


        public UserControl AsControl() => this;

        public void Awake(object obj)
        {
            // TODO: better exception
            if (obj is not bool value)
                throw new Exception();

            Button.IsChecked = value;
            //Button.Click += Button_Click;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Button.IsChecked = !Button.IsChecked;
            //e.Handled = true;

            /*
            ValueChanged?.Invoke(Button.IsChecked ?? false);
            */
        }

        private void Button_Checked(object sender, RoutedEventArgs e)
        {
            ValueChanged?.Invoke(true);
        }

        private void Button_Unchecked(object sender, RoutedEventArgs e)
        {
            ValueChanged?.Invoke(false);
        }
    }
}
