using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbpBalanceModifier : hkbModifier
    {
        public override uint Signature { get => 648393776; }
        
        public bool m_giveUp;
        public float m_comDistThreshold;
        public bool m_passThrough;
        public short m_ragdollLeftFootBoneIndex;
        public short m_ragdollRightFootBoneIndex;
        public float m_balanceOnAnklesFraction;
        public int m_upAxis;
        public float m_fadeInTime;
        public float m_comBiasX;
        public List<hkbpBalanceModifierStepInfo> m_stepInfo;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_giveUp = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_comDistThreshold = br.ReadSingle();
            m_passThrough = br.ReadBoolean();
            br.ReadByte();
            m_ragdollLeftFootBoneIndex = br.ReadInt16();
            m_ragdollRightFootBoneIndex = br.ReadInt16();
            br.ReadUInt16();
            m_balanceOnAnklesFraction = br.ReadSingle();
            m_upAxis = br.ReadInt32();
            m_fadeInTime = br.ReadSingle();
            m_comBiasX = br.ReadSingle();
            m_stepInfo = des.ReadClassArray<hkbpBalanceModifierStepInfo>(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteBoolean(m_giveUp);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_comDistThreshold);
            bw.WriteBoolean(m_passThrough);
            bw.WriteByte(0);
            bw.WriteInt16(m_ragdollLeftFootBoneIndex);
            bw.WriteInt16(m_ragdollRightFootBoneIndex);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_balanceOnAnklesFraction);
            bw.WriteInt32(m_upAxis);
            bw.WriteSingle(m_fadeInTime);
            bw.WriteSingle(m_comBiasX);
            s.WriteClassArray<hkbpBalanceModifierStepInfo>(bw, m_stepInfo);
            bw.WriteUInt64(0);
        }
    }
}
