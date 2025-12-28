using System.Collections.Generic;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle.Network
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class MorphemeNodeDef
    {
        public int nodeType;
        public byte field1_0x4;
        public byte field1_0x5;
        public short field3_0x6;
        //public long arrayOffset;

        public List<byte> array = new List<byte>();
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
