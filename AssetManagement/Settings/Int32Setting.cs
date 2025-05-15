using Shiftless.Clockwork.Assets.Editor.UserControls.Settings;
using Shiftless.Common.Serialization;

namespace Shiftless.Clockwork.Assets.Editor.AssetManagement.Settings
{
    internal class Int32Setting(int value) : AssetSettings.Setting
    {
        // Values
        private int _value = value;


        // Properties
        public override object Value
        {
            get => _value;
            set
            {
                if (value is not int v)
                    throw new InvalidCastException();

                if (_value == v) return;

                _value = v;
            }
        }


        // Func
        public override ISettingControl CreateElement() => new Int32Control();

        public override void Deserialize(ByteStream stream) => _value = stream.ReadInt32();

        public override byte[] Serialize() => ByteConverter.GetBytes(_value);
    }
}
