using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpMouseSpringAction : hkpUnaryAction
    {
        public override uint Signature { get => 1614144929; }
        
        public Vector4 m_positionInRbLocal;
        public Vector4 m_mousePositionInWorld;
        public float m_springDamping;
        public float m_springElasticity;
        public float m_maxRelativeForce;
        public float m_objectDamping;
        public uint m_shapeKey;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_positionInRbLocal = des.ReadVector4(br);
            m_mousePositionInWorld = des.ReadVector4(br);
            m_springDamping = br.ReadSingle();
            m_springElasticity = br.ReadSingle();
            m_maxRelativeForce = br.ReadSingle();
            m_objectDamping = br.ReadSingle();
            m_shapeKey = br.ReadUInt32();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_positionInRbLocal);
            s.WriteVector4(bw, m_mousePositionInWorld);
            bw.WriteSingle(m_springDamping);
            bw.WriteSingle(m_springElasticity);
            bw.WriteSingle(m_maxRelativeForce);
            bw.WriteSingle(m_objectDamping);
            bw.WriteUInt32(m_shapeKey);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
