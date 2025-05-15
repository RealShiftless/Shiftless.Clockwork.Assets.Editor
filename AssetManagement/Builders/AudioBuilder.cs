using Shiftless.Clockwork.Assets.Editor.AssetManagement.Settings;
using Shiftless.Clockwork.Assets.Editor.Audio;
using Shiftless.Common.Serialization;
using Shiftless.SexyAudioFormat;
using Shiftless.SexyAudioFormat.Serialization;
using System.IO;

namespace Shiftless.Clockwork.Assets.Editor.AssetManagement.Builders
{
    internal class AudioBuilder : AssetBuilder
    {
        public override string Name => "Audio";
        public override string FileExtensionPattern => "wav";

        public override string Extension => "ssaf";

        public override (long, long) Encode(string path, string output, AssetSettings settings)
        {
            ByteReader reader = new ByteReader(path);

            AudioInfo info = ReadWavHeader(reader);
            AudioBuffer buffer = new(info.SampleRate, info.Depth, info.Channels, reader.GetRemaining());

            byte[] data = Ssaf.Encode(buffer);

            Directory.CreateDirectory(Path.GetDirectoryName(output) ?? "");
            File.WriteAllBytes(output, data);

            return (reader.Length, data.Length);
        }

        public override AssetProperties GetProperties(string path)
        {
            ByteReader reader = new(path);

            AudioInfo info = ReadWavHeader(reader);
            reader.Dispose();
            return new(info);
        }

        private AudioInfo ReadWavHeader(ByteReader reader)
        {
            // First read the riff header and check if its correct
            string riff = ByteConverter.ToString(reader.Next(4));  // (4 bytes): "RIFF"
            reader.Skip(4);                                     // (4 bytes): file size - 8, I skip this
            string wave = ByteConverter.ToString(reader.Next(4));  // (4 bytes): "WAVE"

            if (riff != "RIFF" || wave != "WAVE")
                throw new FileLoadException($"Invalid wave file: Invalid riff header!");

            // Now for the format chunk
            // First I skip until I actually get there, and throw an error if there is none
            if (!reader.TrySkipUntil(out _, "fmt ", true))
                throw new FileLoadException($"Invalid wave file: No 'fmt ' header found!");

            uint subChunkSize = reader.NextUInt32();
            ushort audioFormat = reader.NextUInt16();

            if (audioFormat != 1 || subChunkSize != 16)
                throw new FileLoadException($"Invalid wave file : Invalid riff header!");

            ushort channelCount = reader.NextUInt16();
            uint sampleRate = reader.NextUInt32();

            reader.Skip(6);

            ushort bitsPerSample = reader.NextUInt16();
            BitDepth bitDepth = (BitDepth)bitsPerSample;

            if (!reader.TrySkipUntil(out _, "data", true))
                throw new FileLoadException($"Invalid wave file : Invalid riff header!");

            reader.Skip(8);

            ulong samples = (ulong)(reader.Remaining / (bitsPerSample / 8) / channelCount);

            return new(sampleRate, bitDepth, samples, channelCount);
        }

        public override AssetSettings CreateSettings()
        {
            return new AudioSettings();
        }

        //public bool IsOfType(string extension) => Regex.IsMatch("", @"\.(wav)$")
    }
}
