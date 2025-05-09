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
    /// Interaction logic for Int32Control.xaml
    /// </summary>
    public partial class Int32Control : UserControl, ISettingControl
    {
        // Events
        public event Action<object>? ValueChanged;


        // Constructor
        public Int32Control()
        {
            InitializeComponent();
        }


        // Func
        public UserControl AsControl() => this;

        public void Awake(object obj)
        {
            if (obj is not int value)
                throw new Exception(); // TODO: better exception

            Body.Text = value.ToString();
        }

        private void Body_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if(!int.TryParse(e.Text, out _))
                e.Handled = true;
        }

        private void Body_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValueChanged?.Invoke(int.Parse(Body.Text));
        }
    }
}
