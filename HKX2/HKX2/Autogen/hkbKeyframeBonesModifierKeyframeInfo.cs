using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbKeyframeBonesModifierKeyframeInfo : IHavokObject
    {
        public virtual uint Signature { get => 1927198630; }
        
        public Vector4 m_keyframedPosition;
        public Quaternion m_keyframedRotation;
        public short m_boneIndex;
        public bool m_isValid;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_keyframedPosition = des.ReadVector4(br);
            m_keyframedRotation = des.ReadQuaternion(br);
            m_boneIndex = br.ReadInt16();
            m_isValid = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_keyframedPosition);
            s.WriteQuaternion(bw, m_keyframedRotation);
            bw.WriteInt16(m_boneIndex);
            bw.WriteBoolean(m_isValid);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
        }
    }
}
