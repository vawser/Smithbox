using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCombineTransformsModifier : hkbModifier
    {
        public override uint Signature { get => 1986827686; }
        
        public Vector4 m_translationOut;
        public Quaternion m_rotationOut;
        public Vector4 m_leftTranslation;
        public Quaternion m_leftRotation;
        public Vector4 m_rightTranslation;
        public Quaternion m_rightRotation;
        public bool m_invertLeftTransform;
        public bool m_invertRightTransform;
        public bool m_invertResult;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_translationOut = des.ReadVector4(br);
            m_rotationOut = des.ReadQuaternion(br);
            m_leftTranslation = des.ReadVector4(br);
            m_leftRotation = des.ReadQuaternion(br);
            m_rightTranslation = des.ReadVector4(br);
            m_rightRotation = des.ReadQuaternion(br);
            m_invertLeftTransform = br.ReadBoolean();
            m_invertRightTransform = br.ReadBoolean();
            m_invertResult = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_translationOut);
            s.WriteQuaternion(bw, m_rotationOut);
            s.WriteVector4(bw, m_leftTranslation);
            s.WriteQuaternion(bw, m_leftRotation);
            s.WriteVector4(bw, m_rightTranslation);
            s.WriteQuaternion(bw, m_rightRotation);
            bw.WriteBoolean(m_invertLeftTransform);
            bw.WriteBoolean(m_invertRightTransform);
            bw.WriteBoolean(m_invertResult);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
        }
    }
}
