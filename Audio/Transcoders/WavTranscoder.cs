using Shiftless.Common.Serialization;
using Shiftless.SexyAudioFormat;
using System.IO;

namespace Shiftless.Clockwork.Assets.Editor.Audio.Transcoders
{
    internal class WavTranscoder : ITranscoder<AudioBuffer>
    {
        public string Extension => ".wav";

        AudioBuffer ITranscoder<AudioBuffer>.Decode(ByteReader reader)
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

            return new(sampleRate, bitDepth, channelCount, reader.GetRemaining());
        }

        byte[] ITranscoder<AudioBuffer>.Encode(AudioBuffer obj)
        {
            throw new NotImplementedException();
        }
    }
}
