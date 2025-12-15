using System.Collections.Generic;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle.Network
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class NodeDef
    {
        public NodeType nodeTypeID = NodeType.NodeType_NetworkInstance;
        public byte flags1;
        public byte flags2;
        public short nodeId;
        public short parentNodeId;
        public short numChildNodeIDs;
        public short field7_0xc;
        public byte numControlParamAndOpNodeIDs;
        public byte field8_0xf;
        public short numDataSet;
        public short field10_0x12;
        public int padding;
        public long owningNetworkDef;
        public List<short> childNodeIds = new List<short>();
        public List<int> controlParamAndOpNodeIDs = new List<int>();
        //public long nodeDataOffset;
        public List<NodeDataSet> nodeData = new List<NodeDataSet>();
        public short field16_0x38;
        public short field17_0x3A;
        public int field18_0x3C;
        public long field19_0x40;
        public long field20_0x48;
        public long deleteFunctionOffset;
        public long updateFunctionOffset;
        public long unknownFunctionOffset;
        public long initFunctionOffset;
        public long transitFunctionOffset;
        //public long MorphemeNodeDef nodeDefOffset;
        public MorphemeNodeDef nodeDef;
        public byte field27_0x80;
        public int padding1;
        public short padding2;
        public byte padding3;
        public long field35_0x88;
        public long field36_0x8C;
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
