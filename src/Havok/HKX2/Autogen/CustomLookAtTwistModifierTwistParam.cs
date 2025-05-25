using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class CustomLookAtTwistModifierTwistParam : IHavokObject
    {
        public virtual uint Signature { get => 1192258791; }
        
        public short m_startBoneIndex;
        public short m_endBoneIndex;
        public float m_targetRotationRate;
        public float m_newTargetGain;
        public float m_onGain;
        public float m_offGain;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_startBoneIndex = br.ReadInt16();
            m_endBoneIndex = br.ReadInt16();
            m_targetRotationRate = br.ReadSingle();
            m_newTargetGain = br.ReadSingle();
            m_onGain = br.ReadSingle();
            m_offGain = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt16(m_startBoneIndex);
            bw.WriteInt16(m_endBoneIndex);
            bw.WriteSingle(m_targetRotationRate);
            bw.WriteSingle(m_newTargetGain);
            bw.WriteSingle(m_onGain);
            bw.WriteSingle(m_offGain);
            bw.WriteUInt32(0);
        }
    }
}
