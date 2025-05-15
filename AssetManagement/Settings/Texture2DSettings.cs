using Shiftless.Clockwork.Assets.Editor.Mathematics;

namespace Shiftless.Clockwork.Assets.Editor.AssetManagement.Settings
{
    public class Texture2DSettings : AssetSettings
    {
        public ColorMode ColorMode => Get<ColorMode>("color_mode");

        public bool FlipY => Get<bool>("flip_y");

        public bool IsTileset => Get<bool>("is_tileset");
        public int TileWidth => Get<int>("tile_width");
        public int TileHeight => Get<int>("tile_height");

        public bool StoresPalette => Get<bool>("stores_palette");
        public bool AutoPalette => Get<bool>("auto_palette");

        internal Texture2DSettings()
        {
            AddSetting("color_mode", ColorMode.RGBA8);

            AddSetting("flip_y", true);

            AddSetting("is_tileset", false);

            AddSetting("tile_width", 8, () => IsTileset);
            AddSetting("tile_height", 8, () => IsTileset);

            AddSetting("stores_palette", true, () =>
            {
                ColorMode colorMode = ColorMode;
                return colorMode >= ColorMode.Palette1 && colorMode <= ColorMode.Palette8;
            });

            AddSetting("auto_palette", true, () =>
            {
                ColorMode colorMode = ColorMode;
                return colorMode >= ColorMode.Palette1 && colorMode <= ColorMode.Palette8 && StoresPalette;
            });



            for (byte i = 0; i < byte.MaxValue; i++)
            {
                Func<bool> visibleFunc;
                if (i < 2)
                {
                    visibleFunc = () =>
                    {
                        ColorMode colorMode = ColorMode;
                        return colorMode >= ColorMode.Palette1 && colorMode <= ColorMode.Palette8 && StoresPalette && !AutoPalette;
                    };
                }
                else if (i < 4)
                {
                    visibleFunc = () =>
                    {
                        ColorMode colorMode = ColorMode;
                        return colorMode >= ColorMode.Palette2 && colorMode <= ColorMode.Palette8 && StoresPalette && !AutoPalette;
                    };
                }
                else if (i < 16)
                {
                    visibleFunc = () =>
                    {
                        ColorMode colorMode = ColorMode;
                        return colorMode >= ColorMode.Palette4 && colorMode <= ColorMode.Palette8 && StoresPalette && !AutoPalette;
                    };
                }
                else
                {
                    visibleFunc = () =>
                    {
                        ColorMode colorMode = ColorMode;
                        return colorMode == ColorMode.Palette8 && StoresPalette && !AutoPalette;
                    };
                }

                AddSetting($"palette_{i}", new Color(i, i, i, 255), visibleFunc);
            }
        }
    }
}
