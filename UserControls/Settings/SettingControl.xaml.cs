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
    /// Interaction logic for SettingControl.xaml
    /// </summary>
    public partial class SettingControl : UserControl
    {
        public event Action<string, object>? SettingChanged;

        public event Action<object>? OnAwake;

        internal SettingControl(string name, ISettingControl setting)
        {
            InitializeComponent();

            NameBlock.Text = name;
            Body.Child = setting.AsControl();

            setting.ValueChanged += Setting_ValueChanged;

            OnAwake += setting.Awake;
        }

        internal void Awake(object value) => OnAwake?.Invoke(value);

        private void Setting_ValueChanged(object obj) => SettingChanged?.Invoke(NameBlock.Text, obj);
    }
}
