using Shiftless.Common.Serialization;

namespace Shiftless.Clockwork.Assets.Editor.Audio.Transcoders
{
    public interface ITranscoder<T>
    {
        string Extension { get; }

        byte[] Encode(T obj);
        T Decode(ByteReader reader);
    }
}
