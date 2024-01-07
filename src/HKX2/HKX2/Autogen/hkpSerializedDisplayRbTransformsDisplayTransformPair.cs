using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpSerializedDisplayRbTransformsDisplayTransformPair : IHavokObject
    {
        public virtual uint Signature { get => 2494323692; }
        
        public hkpRigidBody m_rb;
        public Matrix4x4 m_localToDisplay;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_rb = des.ReadClassPointer<hkpRigidBody>(br);
            br.ReadUInt64();
            m_localToDisplay = des.ReadTransform(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hkpRigidBody>(bw, m_rb);
            bw.WriteUInt64(0);
            s.WriteTransform(bw, m_localToDisplay);
        }
    }
}
