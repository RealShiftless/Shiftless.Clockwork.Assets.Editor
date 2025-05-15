using Shiftless.Clockwork.Assets.Editor.UserControls.Settings;
using Shiftless.Common.Serialization;

namespace Shiftless.Clockwork.Assets.Editor.AssetManagement.Settings
{
    internal class BoolSetting(bool value) : AssetSettings.Setting
    {
        private bool _value = value;

        public override object Value
        {
            get => _value;
            set
            {
                if (value is not bool boolValue)
                    throw new InvalidCastException($"Value of type {value.GetType().Name} could not be cast to bool!");

                _value = boolValue;
            }
        }

        public override ISettingControl CreateElement() => new BoolControl();

        public override void Deserialize(ByteStream stream)
        {
            _value = stream.Read() != 0;
        }

        public override byte[] Serialize()
        {
            return [(byte)(_value ? 1 : 0)];
        }
    }
}
