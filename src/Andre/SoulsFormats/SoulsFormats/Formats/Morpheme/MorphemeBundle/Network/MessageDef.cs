using System.Collections.Generic;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle.Network
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class MessageDef
    {
        public int messageId;
        public int field1_0x4;
        public int nodeCount;
        public int field3_0xC;
        //public long nodeArrayOffset;

        public List<short> nodeArray = new List<short>();
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
