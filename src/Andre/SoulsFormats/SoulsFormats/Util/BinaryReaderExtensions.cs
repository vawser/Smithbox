using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats
{
    public static class BinaryReaderWriterExtensions
    {
        public static long GetNextPaddedOffsetAfterCurrentField(this BinaryReaderEx br, int currentFieldLength, int align)
        {
            long pos = br.Position;
            pos += currentFieldLength;
            if (align <= 0)
                return pos;
            if (pos % align > 0)
                pos += align - (pos % align);
            return pos;
        }
    }
}