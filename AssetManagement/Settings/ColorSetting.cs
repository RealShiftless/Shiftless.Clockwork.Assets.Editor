using Shiftless.Clockwork.Assets.Editor.Mathematics;
using Shiftless.Clockwork.Assets.Editor.UserControls.Settings;
using Shiftless.Common.Serialization;

namespace Shiftless.Clockwork.Assets.Editor.AssetManagement.Settings
{
    internal class ColorSetting : AssetSettings.Setting
    {
        // Values
        private Color _color;


        // Constructor
        public ColorSetting(Color value)
        {
            _color = value;
        }


        // Properties
        public override object Value
        {
            get => _color;
            set
            {
                if (value is not Color colorValue)
                    throw new Exception(); // TODO: better exception

                _color = colorValue;
            }
        }


        // Func
        public override ISettingControl CreateElement() => new ColorControl();

        public override void Deserialize(ByteStream stream) => _color = stream.ReadUInt32();

        public override byte[] Serialize() => ByteConverter.GetBytes(_color);
    }
}
