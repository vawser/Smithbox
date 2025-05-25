using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpCdBody : IHavokObject
    {
        public virtual uint Signature { get => 1420081217; }
        
        public hkpShape m_shape;
        public uint m_shapeKey;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_shape = des.ReadClassPointer<hkpShape>(br);
            m_shapeKey = br.ReadUInt32();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hkpShape>(bw, m_shape);
            bw.WriteUInt32(m_shapeKey);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
