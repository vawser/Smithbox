using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpListShapeChildInfo : IHavokObject
    {
        public virtual uint Signature { get => 563350563; }
        
        public hkpShape m_shape;
        public uint m_collisionFilterInfo;
        public ushort m_shapeInfo;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_shape = des.ReadClassPointer<hkpShape>(br);
            m_collisionFilterInfo = br.ReadUInt32();
            m_shapeInfo = br.ReadUInt16();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hkpShape>(bw, m_shape);
            bw.WriteUInt32(m_collisionFilterInfo);
            bw.WriteUInt16(m_shapeInfo);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
        }
    }
}
