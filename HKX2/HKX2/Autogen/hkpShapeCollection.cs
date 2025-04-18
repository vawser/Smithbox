using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum CollectionType
    {
        COLLECTION_LIST = 0,
        COLLECTION_EXTENDED_MESH = 1,
        COLLECTION_TRISAMPLED_HEIGHTFIELD = 2,
        COLLECTION_USER = 3,
        COLLECTION_SIMPLE_MESH = 4,
        COLLECTION_MESH_SHAPE = 5,
        COLLECTION_COMPRESSED_MESH = 6,
        COLLECTION_MAX = 7,
    }
    
    public partial class hkpShapeCollection : hkpShape
    {
        public override uint Signature { get => 154959627; }
        
        public bool m_disableWelding;
        public CollectionType m_collectionType;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_disableWelding = br.ReadBoolean();
            m_collectionType = (CollectionType)br.ReadByte();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteBoolean(m_disableWelding);
            bw.WriteByte((byte)m_collectionType);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
