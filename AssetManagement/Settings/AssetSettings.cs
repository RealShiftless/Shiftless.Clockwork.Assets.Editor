using Shiftless.Clockwork.Assets.Editor.Mathematics;
using Shiftless.Clockwork.Assets.Editor.UserControls.Settings;
using Shiftless.Common.Serialization;
using System.Collections;

namespace Shiftless.Clockwork.Assets.Editor.AssetManagement.Settings
{
    public abstract class AssetSettings : IEnumerable<KeyValuePair<string, AssetSettings.Setting>>, IByteSerializable
    {
        // Values
        private static Dictionary<Type, SettingsPanel> _settingPanels = []; // I use this one to pre make setting panels, so I can also reuse them for different

        private Dictionary<string, Setting> _settings = [];


        // Func
        public T Get<T>(string name)
        {
            if (!_settings.TryGetValue(name, out var setting))
                throw new KeyNotFoundException($"Could not find setting of name {name}!");

            object obj = _settings[name].Value;
            if (obj is not T value)
                throw new InvalidCastException($"Could not cast setting of type {obj.GetType().Name} to {typeof(T).Name}");

            return value;
        }

        public SettingsPanel GetPanel()
        {
            if (_settingPanels.TryGetValue(GetType(), out SettingsPanel? panel))
                return _settingPanels[GetType()];

            panel = new(this);
            _settingPanels.Add(GetType(), panel);

            return panel;
        }

        internal void Set(string name, object value) => _settings[name].Value = value;

        private void AddSetting(string name, Setting setting, Func<bool>? isVisibleFunc)
        {
            _settings.Add(name, setting);
            setting.IsVisibleFunc = isVisibleFunc;
        }

        protected void AddSetting(string name, int value, Func<bool>? isVisibleFunc = null) => AddSetting(name, new Int32Setting(value), isVisibleFunc);
        protected void AddSetting<T>(string name, T value, Func<bool>? isVisibleFunc = null) where T : Enum => AddSetting(name, new EnumSetting<T>(value), isVisibleFunc);
        protected void AddSetting(string name, bool value, Func<bool>? isVisibleFunc = null) => AddSetting(name, new BoolSetting(value), isVisibleFunc);
        protected void AddSetting(string name, Color value, Func<bool>? isVisibleFunc = null) => AddSetting(name, new ColorSetting(value), isVisibleFunc);

        public bool IsSettingVisible(string name) => _settings[name].IsVisible;


        // Interface
        IEnumerator<KeyValuePair<string, Setting>> IEnumerable<KeyValuePair<string, Setting>>.GetEnumerator() => _settings.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _settings.GetEnumerator();

        public byte[] Serialize()
        {
            ByteWriter writer = new ByteWriter();
            foreach ((string name, Setting setting) in _settings)
            {
                writer.Write(name);
                writer.Write(setting.Serialize());
                writer.Write((byte)0);
            }

            writer.Write((byte)0);

            return writer.ToArray();
        }

        public void Deserialize(ByteStream stream)
        {
            while (stream.Peek() != 0)
            {
                string name = stream.ReadString();

                if (_settings.TryGetValue(name, out var setting))
                    setting.Deserialize(stream);
                else
                    stream.ReadUntil([0]);

                stream.Position++;
            }

            stream.Position++;
        }


        // Classes
        public abstract class Setting : IByteSerializable
        {
            // Values
            internal Func<bool>? IsVisibleFunc;


            // Properties
            public abstract object Value { get; set; }

            public bool IsVisible => IsVisibleFunc?.Invoke() ?? true;


            // Abstracts
            public abstract ISettingControl CreateElement();
            public abstract void Deserialize(ByteStream stream);
            public abstract byte[] Serialize();
        }

    }
}
