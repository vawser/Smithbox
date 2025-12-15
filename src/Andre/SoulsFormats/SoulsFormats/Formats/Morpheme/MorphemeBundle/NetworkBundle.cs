using SoulsFormats.Formats.Morpheme.MorphemeBundle.Network;
using System;
using System.Collections.Generic;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class NetworkBundle : MorphemeBundle_Base
    {
        public NodeDef networkNodeDef;
        public int numNodes;
        public int field2_0x94;
        //public long nodesOffset;
        public List<NodeDef> nodes = new List<NodeDef>();
        public short field4_0xa0;
        public short field5_0xa2;
        public int field6_0xa4;
        public long field7_0xa8;
        //public long smStatelistOffset;
        public SmStateList smStateList;
        public long field9_0xb8;
        //public long smUnklistOffset;
        public SmStateList smUnkList;
        //public long nodeIDNamesTableOffset;
        public LookupTable nodeIDNamesTable;
        //public long requestIDNamesTableOffset;
        public LookupTable requestIDNamesTable;
        //public long eventTrackIDNamesTableOffset;
        public LookupTable eventTrackIDNamesTable;
        public long field14_0xe0;
        public long field15_0xe8;
        public long field16_0xf0;
        public int messageCount;
        public int field18_0xfc;
        //public long messageDefOffset;
        public List<MessageDef> messageDefList = new List<MessageDef>();
        public int nodeTypeCount;
        public int field21_0x10c;
        //public long nodeDefListOffset;
        public List<MorphemeNodeDef> nodeDefList = new List<MorphemeNodeDef>();
        public int field23_0x118;
        public int field24_0x11c;
        public int field25_0x120;
        public int field26_0x124;
        public int field27_0x128;
        public int field28_0x12c;
        public long field29_0x130;
        public long field30_0x138;

        public override long CalculateBundleSize()
        {
            throw new NotImplementedException();
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
