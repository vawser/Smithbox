using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum BvTreeType
    {
        BVTREE_MOPP = 0,
        BVTREE_TRISAMPLED_HEIGHTFIELD = 1,
        BVTREE_STATIC_COMPOUND = 2,
        BVTREE_COMPRESSED_MESH = 3,
        BVTREE_USER = 4,
        BVTREE_MAX = 5,
    }
    
    public partial class hkpBvTreeShape : hkpShape
    {
        public override uint Signature { get => 334912360; }
        
        public BvTreeType m_bvTreeType;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_bvTreeType = (BvTreeType)br.ReadByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteByte((byte)m_bvTreeType);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
