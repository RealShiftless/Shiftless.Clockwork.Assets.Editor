using Shiftless.SexyAudioFormat;

namespace Shiftless.Clockwork.Assets.Editor.Audio
{
    public readonly struct AudioInfo(uint sampleRate, BitDepth depth, ulong totalSamples, ushort channels)
    {
        public readonly uint SampleRate = sampleRate;
        public readonly BitDepth Depth = depth;
        public readonly ulong TotalSamples = totalSamples;
        public readonly ushort Channels = channels;

        public readonly string Duration
        {
            get
            {
                long totalSeconds = (long)TotalSamples / SampleRate;
                long totalMinutes = totalSeconds / 60;

                int seconds = (int)(totalSeconds % 60);
                int minutes = (int)(totalMinutes % 60);
                int hours = (int)(totalMinutes / 60);

                return hours > 0 ?
                    $"{hours}:{minutes:D2}:{seconds:D2}" :
                    $"{minutes:D2}:{seconds:D2}";
            }
        }
    }
}
