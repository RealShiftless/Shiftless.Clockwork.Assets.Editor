namespace Shiftless.Clockwork.Assets.Editor.Mathematics
{
    public struct Color
    {
        private uint _value;

        public byte R
        {
            get => (byte)(_value >> 24 & 0xFF);
            set
            {
                _value &= 0x00FFFFFF;
                _value = (uint)value << 24 | _value;
            }
        }
        public byte G
        {
            get => (byte)(_value >> 16 & 0xFF);
            set
            {
                _value &= 0xFF00FFFF;
                _value = (uint)value << 16 | _value;
            }
        }
        public byte B
        {
            get => (byte)(_value >> 8 & 0xFF);
            set
            {
                _value &= 0xFFFF00FF;
                _value = (uint)value << 8 | _value;
            }
        }
        public byte A
        {
            get => (byte)(_value >> 0 & 0xFF);
            set
            {
                _value &= 0xFFFFFF00;
                _value |= value;
            }
        }

        public uint Value
        {
            get => _value;
            set => _value = value;
        }

        public Color(byte r, byte g, byte b, byte a)
        {
            _value = (uint)r << 24 | (uint)g << 16 | (uint)b << 8 | a;
        }
        public Color(uint color)
        {
            _value = color;
        }


        public static implicit operator Color(uint value) => new(value);
        public static implicit operator uint(Color value) => value._value;

        public static implicit operator System.Windows.Media.Color(Color value) => System.Windows.Media.Color.FromArgb(value.A, value.R, value.G, value.B);

        public override readonly string ToString() => _value.ToString("X8");
    }
}
