using Shiftless.Clockwork.Assets.Editor.AssetManagement.Settings;
using Shiftless.Common.Serialization;
using StbImageSharp;
using System.IO;

namespace Shiftless.Clockwork.Assets.Editor.AssetManagement.Builders
{
    internal class Texture2DBuilder : AssetBuilder
    {
        private static Dictionary<ColorMode, Action<ByteWriter, Texture2DSettings, ImageResult>> _colorModes = new()
        {
            { ColorMode.Lum1, (writer, settings, image) => EncodeBits(writer, settings, image, PixelsPerByte.Ppb8)},
            { ColorMode.Lum2, (writer, settings, image) => EncodeBits(writer, settings, image, PixelsPerByte.Ppb4)},
            { ColorMode.Lum4, (writer, settings, image) => EncodeBits(writer, settings, image, PixelsPerByte.Ppb2)},
            { ColorMode.Lum8, (writer, settings, image) => EncodeBits(writer, settings, image, PixelsPerByte.Ppb1)},

            { ColorMode.LumA8, EncodeLumA8},

            { ColorMode.Palette1, (writer, settings, image) => EncodeBits(writer, settings, image, PixelsPerByte.Ppb8)},
            { ColorMode.Palette2, (writer, settings, image) => EncodeBits(writer, settings, image, PixelsPerByte.Ppb4)},
            { ColorMode.Palette4, (writer, settings, image) => EncodeBits(writer, settings, image, PixelsPerByte.Ppb2)},
            { ColorMode.Palette8, (writer, settings, image) => EncodeBits(writer, settings, image, PixelsPerByte.Ppb1)},

            { ColorMode.RGB565, EncodeRGB565 },
            { ColorMode.RGB8, EncodeRGB8 },
            { ColorMode.RGBA8, EncodeRGBA8 },
        };

        public override string Name => "Texture";

        public override string FileExtensionPattern => "jpg|jpeg|png|tga|bmp|psd|hdr|pic|ppm|pgm";

        public override string Extension => "bin";

        public override (long, long) Encode(string path, string output, AssetSettings assetSettings)
        {
            if (assetSettings is not Texture2DSettings settings)
                throw new ArgumentException("assetSettings was not of type Texture2DSettings");

            using var stream = File.OpenRead(path);
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.Grey);

            ByteWriter writer = new();

            ColorMode colorMode = settings.ColorMode;

            writer.Write("Tx2D", null);
            writer.Write((uint)image.Width);
            writer.Write((uint)image.Height);
            writer.Write((int)colorMode);

            if (colorMode >= ColorMode.Palette1 && colorMode <= ColorMode.Palette8 && settings.StoresPalette)
            {
                if (settings.AutoPalette)
                    throw new NotImplementedException("Auto palette not implemented!");

                writer.Write("plte", null);

                int offset = colorMode - ColorMode.Palette1;
                int colors = 1 << (offset << 1);

                for (int i = 0; i < colors; i++)
                {
                    writer.Write(settings.Get<Mathematics.Color>($"palette_{i}"));
                }
            }

            writer.Write("data", null);
            _colorModes[settings.ColorMode].Invoke(writer, settings, image);

            long sourceLength = new FileInfo(path).Length;

            writer.Save(output);

            return (sourceLength, writer.Length);
        }

        public override AssetProperties GetProperties(string path)
        {
            using var stream = File.OpenRead(path);
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.Grey);
            return new AssetProperties(
                ("Width", image.Width),
                ("Height", image.Height),
                ("Channels", image.SourceComp));
        }

        public override AssetSettings CreateSettings()
        {
            return new Texture2DSettings();
        }

        private static void EncodeIndexData(ByteWriter writer, ImageResult image, PixPerByte pixPerByte)
        {
            int components = (int)image.Comp;
            if (components == 0)
                components = 4;

            int pixelsPerByte = (int)pixPerByte;
            int bitsPerPixel = 8 / pixelsPerByte;
            int maxValue = (1 << bitsPerPixel) - 1;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x += (int)pixPerByte)
                {
                    byte b = 0;

                    for (int i = 0; i < pixelsPerByte; i++)
                    {
                        uint value = 0;
                        for (int j = 0; j < components; j++)
                        {
                            int index = ((y * image.Width) + (x + i)) * components + j;
                            value += image.Data[index];
                        }

                        byte avg = (byte)(value / components);
                        byte pixelValue = (byte)((avg * maxValue + 127) / 255);

                        int shift = 8 - bitsPerPixel * (i + 1);
                        b |= (byte)(pixelValue << shift);
                    }

                    writer.Write(b);
                }
            }
        }
        //                                                                                        index    y    r     g     b     a
        private static void EnumeratePixels(Texture2DSettings settings, ImageResult image, Action<int, byte, byte, byte, byte> enumerateFunc)
        {
            int components = (int)image.Comp;
            if (components == 0)
                components = 4;

            bool isTileset = settings.IsTileset;
            int tileWidth = settings.TileWidth;
            int tileHeight = settings.TileHeight;

            if (isTileset && (image.Width % tileWidth != 0 || image.Height != tileHeight))
                throw new Exception("Tileset sizing was incorrect!");


            for (int i = 0; i < image.Data.Length / components; i++)
            {
                int index = CalculatePixelIndex(i, isTileset, image.Width, image.Height, tileWidth, tileHeight, settings.FlipY);

                (byte r, byte g, byte b, byte a) = GetPixel(image, index, components);

                enumerateFunc(i, r, g, b, a);
            }

            /*
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    byte r = 0;
                    byte g = 0;
                    byte b = 0;
                    byte a = 0;

                    for (int j = 0; j < components; j++)
                    {
                        int index = ((y * image.Width) + x) * components + j;
                        byte v = image.Data[index];

                        if(components == 1)
                        {
                            r = v;
                            g = v;
                            b = v;
                            a = 255;
                        }
                        else if(components == 2)
                        {
                            switch(j)
                            {
                                case 0:
                                    r = v;
                                    g = v;
                                    b = v;
                                    break;

                                case 1:
                                    a = v;
                                    break;
                            }
                        }
                        else if(components == 3)
                        {
                            switch (j)
                            {
                                case 0:
                                    r = v;
                                    break;

                                case 1:
                                    g = v;
                                    break;

                                case 2:
                                    b = v;
                                    a = 255;
                                    break;
                            }
                        }
                        else if(components == 4)
                        {
                            switch (j)
                            {
                                case 0:
                                    r = v;
                                    break;

                                case 1:
                                    b = v;
                                    break;

                                case 2:
                                    g = v;
                                    break;

                                case 3:
                                    a = v;
                                    break;
                            }
                        }
                    }

                    enumerateFunc.Invoke(r, g, b, a);
                }
            }
            */
        }
        private static (byte r, byte g, byte b, byte a) GetPixel(ImageResult image, int index, int components)
        {
            // First make the index an actual index based on the components
            index *= components;

            // Store the rgba already
            byte r = 0;
            byte g = 0;
            byte b = 0;
            byte a = 0;

            for (int j = 0; j < components; j++)
            {
                //int index = ((y * image.Width) + x) * components + j;
                byte v = image.Data[index];

                if (components == 1)
                {
                    r = v;
                    g = v;
                    b = v;
                    a = 255;
                }
                else if (components == 2)
                {
                    switch (j)
                    {
                        case 0:
                            r = v;
                            g = v;
                            b = v;
                            break;

                        case 1:
                            a = v;
                            break;
                    }
                }
                else if (components == 3)
                {
                    switch (j)
                    {
                        case 0:
                            r = v;
                            break;

                        case 1:
                            g = v;
                            break;

                        case 2:
                            b = v;
                            a = 255;
                            break;
                    }
                }
                else if (components == 4)
                {
                    switch (j)
                    {
                        case 0:
                            r = v;
                            break;

                        case 1:
                            b = v;
                            break;

                        case 2:
                            g = v;
                            break;

                        case 3:
                            a = v;
                            break;
                    }
                }
            }

            return (r, g, b, a);
        }
        private static int CalculatePixelIndex(int index, bool isTileset, int width, int height, int tileWidth, int tileHeight, bool flipsY)
        {
            // Non tileset mode
            if (!isTileset)
            {
                if (!flipsY)
                    return index;

                int x = index % width;
                int y = index / width;
                if (flipsY) y = height - y - 1;
                return y * width + x;
            }

            // Tileset mode
            int tileSize = tileWidth * tileHeight;
            int tileIndex = index / tileSize;
            int localIndex = index % tileSize;

            int tilesPerRow = width / tileWidth;
            int tileX = tileIndex % tilesPerRow;
            int tileY = tileIndex / tilesPerRow;

            int localX = localIndex % tileWidth;
            int localY = localIndex / tileWidth;
            if (flipsY) localY = tileHeight - localY - 1;

            int pixelX = tileX * tileWidth + localX;
            int pixelY = tileY * tileHeight + localY;
            return pixelY * width + pixelX;
        }

        private static void EncodeBits(ByteWriter writer, Texture2DSettings settings, ImageResult image, PixelsPerByte ppb)
        {
            int pixelsPerByte = (int)ppb;
            int bitsPerPixel = 8 / pixelsPerByte;
            int maxValue = (1 << bitsPerPixel) - 1;

            byte curByte = 0;

            int pixelByteIndex = 0;
            EnumeratePixels(settings, image, (i, r, g, b, a) =>
            {
                byte avg = (byte)((r + g + b) / 3);
                byte pixelValue = (byte)((avg * maxValue + 127) / 255);

                int shift = (pixelsPerByte - pixelByteIndex - 1) * bitsPerPixel;
                curByte |= (byte)(pixelValue << shift);

                pixelByteIndex++;
                if (pixelByteIndex == pixelsPerByte)
                {
                    writer.Write(curByte);
                    curByte = 0;
                    pixelByteIndex = 0;
                }
            });
        }

        private static void EncodeLumA8(ByteWriter writer, Texture2DSettings settings, ImageResult image) => EnumeratePixels(settings, image, (i, r, g, b, a) =>
        {
            byte avg = (byte)((r + g + b) / 3);
            writer.Write(avg);
            writer.Write(a);
        });

        private static void EncodeRGB565(ByteWriter writer, Texture2DSettings settings, ImageResult image) => EnumeratePixels(settings, image, (i, r, g, b, a) =>
        {
            ushort r5 = (ushort)((r * 31 + 127) / 255);  // 5-bit (0–31)
            ushort g6 = (ushort)((g * 63 + 127) / 255);  // 6-bit (0–63)
            ushort b5 = (ushort)((b * 31 + 127) / 255);  // 5-bit (0–31)

            ushort value = (ushort)((r5 << 11) | (g6 << 5) | b5);

            writer.Write(value);
        });

        private static void EncodeRGB8(ByteWriter writer, Texture2DSettings settings, ImageResult image) => EnumeratePixels(settings, image, (i, r, g, b, a) =>
        {
            writer.Write(r);
            writer.Write(g);
            writer.Write(b);
        });
        private static void EncodeRGBA8(ByteWriter writer, Texture2DSettings settings, ImageResult image) => EnumeratePixels(settings, image, (i, r, g, b, a) =>
        {
            writer.Write(r);
            writer.Write(g);
            writer.Write(b);
            writer.Write(a);
        });

        private enum PixPerByte
        {
            Ppb1 = 1,
            Ppb2 = 2,
            Ppb4 = 4,
            Ppb8 = 8
        }
    }
}
