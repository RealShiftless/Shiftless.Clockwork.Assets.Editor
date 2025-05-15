using System.Windows.Controls;

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
