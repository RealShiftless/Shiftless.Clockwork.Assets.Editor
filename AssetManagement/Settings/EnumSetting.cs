using Shiftless.Clockwork.Assets.Editor.UserControls.Settings;
using Shiftless.Common.Serialization;

namespace Shiftless.Clockwork.Assets.Editor.AssetManagement.Settings
{
    public class EnumSetting<T> : AssetSettings.Setting where T : Enum
    {
        // Values
        private T _value;


        // Properties
        public override object Value
        {
            get => _value;
            set
            {
                if (value is not T val) return;

                _value = val;
            }
        }


        // Constructor
        public EnumSetting(T value)
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException($"Value {value.ToString()} was not an enum!");

            _value = value;
        }


        // Func
        public override ISettingControl CreateElement() => new EnumControl();

        public override void Deserialize(ByteStream stream)
        {
            int idValue = stream.ReadInt32();

            _value = (T)Enum.ToObject(typeof(T), idValue);
        }

        public override byte[] Serialize()
        {
            int value = Convert.ToInt32(_value);
            return ByteConverter.GetBytes(value);
        }
    }
}
