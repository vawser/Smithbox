using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbTransformVectorModifier : hkbModifier
    {
        public override uint Signature { get => 3293194348; }
        
        public Quaternion m_rotation;
        public Vector4 m_translation;
        public Vector4 m_vectorIn;
        public Vector4 m_vectorOut;
        public bool m_rotateOnly;
        public bool m_inverse;
        public bool m_computeOnActivate;
        public bool m_computeOnModify;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_rotation = des.ReadQuaternion(br);
            m_translation = des.ReadVector4(br);
            m_vectorIn = des.ReadVector4(br);
            m_vectorOut = des.ReadVector4(br);
            m_rotateOnly = br.ReadBoolean();
            m_inverse = br.ReadBoolean();
            m_computeOnActivate = br.ReadBoolean();
            m_computeOnModify = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteQuaternion(bw, m_rotation);
            s.WriteVector4(bw, m_translation);
            s.WriteVector4(bw, m_vectorIn);
            s.WriteVector4(bw, m_vectorOut);
            bw.WriteBoolean(m_rotateOnly);
            bw.WriteBoolean(m_inverse);
            bw.WriteBoolean(m_computeOnActivate);
            bw.WriteBoolean(m_computeOnModify);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
