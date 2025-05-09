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
    /// Interaction logic for EnumControl.xaml
    /// </summary>
    public partial class EnumControl : UserControl, ISettingControl
    {
        // Events
        public event Action<object>? ValueChanged;

        
        // Constructor
        public EnumControl()
        {
            InitializeComponent();

            Body.SelectionChanged += Body_SelectionChanged;
        }


        // Func
        void ISettingControl.Awake(object obj)
        {
            if (obj is not Enum value)
                throw new InvalidCastException("Invalid value found in settings ui!");

            Body.Items.Clear();

            int i = 0;
            foreach(Enum enumValue in Enum.GetValues(value.GetType()))
            {
                if(Equals(enumValue, value))
                    Body.SelectedIndex = i;

                Body.Items.Add(enumValue);
                i++;
            }

        }

        UserControl ISettingControl.AsControl() => this;


        // Callbacks
        private void Body_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValueChanged?.Invoke(Body.SelectedValue);
        }
    }
}
