using Shiftless.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shiftless.Clockwork.Assets.Editor.Audio.Transcoders
{
    public interface ITranscoder<T>
    {
        string Extension { get; }

        byte[] Encode(T obj);
        T Decode(ByteReader reader);
    }
}
