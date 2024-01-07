using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpStaticCompoundShapeInstance : IHavokObject
    {
        public virtual uint Signature { get => 2584457571; }
        
        public Matrix4x4 m_transform;
        public hkpShape m_shape;
        public uint m_filterInfo;
        public uint m_childFilterInfoMask;
        public ulong m_userData;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_transform = des.ReadQSTransform(br);
            m_shape = des.ReadClassPointer<hkpShape>(br);
            m_filterInfo = br.ReadUInt32();
            m_childFilterInfoMask = br.ReadUInt32();
            m_userData = br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteQSTransform(bw, m_transform);
            s.WriteClassPointer<hkpShape>(bw, m_shape);
            bw.WriteUInt32(m_filterInfo);
            bw.WriteUInt32(m_childFilterInfoMask);
            bw.WriteUInt64(m_userData);
            bw.WriteUInt64(0);
        }
    }
}
