using Shiftless.Clockwork.Assets.Editor.AssetManagement.Settings;
using System.Windows;
using System.Windows.Controls;

namespace Shiftless.Clockwork.Assets.Editor.UserControls.Settings
{
    /// <summary>
    /// Interaction logic for SettingsPanel.xaml
    /// </summary>
    public partial class SettingsPanel : UserControl
    {
        private AssetSettings? _current;

        private Dictionary<string, SettingControl> _settings = [];

        internal SettingsPanel(AssetSettings settings)
        {
            InitializeComponent();

            foreach ((string name, AssetSettings.Setting setting) in settings)
            {
                ISettingControl controlInterface = setting.CreateElement();
                SettingControl control = new(name, controlInterface);

                Body.Children.Add(control);

                _settings.Add(name, control);

                control.SettingChanged += SettingChanged;
            }

            Bind(settings);
        }

        internal void Bind(AssetSettings? settings)
        {
            _current = settings;

            if (_current == null)
                return;

            foreach ((string name, AssetSettings.Setting setting) in _current)
            {
                _settings[name].Awake(setting.Value);
            }
            CheckSettingsVisible();
        }

        private void SettingChanged(string name, object value)
        {
            _current?.Set(name, value);
            CheckSettingsVisible();
        }

        private void CheckSettingsVisible()
        {
            if (_current == null) return;

            foreach ((string name, SettingControl settingControl) in _settings)
                settingControl.Visibility = _current.IsSettingVisible(name) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
