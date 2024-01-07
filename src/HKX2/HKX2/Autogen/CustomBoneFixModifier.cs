using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class CustomBoneFixModifier : hkbModifier
    {
        public override uint Signature { get => 2838570989; }
        
        public enum GainState
        {
            GainStateTargetGain = 0,
            GainStateOn = 1,
            GainStateOff = 2,
            GainStateAnimFix = 3,
        }
        
        public Vector4 m_targetWS;
        public short m_targetBoneIndex;
        public float m_newTargetGain;
        public float m_onGain;
        public float m_offGain;
        public bool m_isOk;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_targetWS = des.ReadVector4(br);
            m_targetBoneIndex = br.ReadInt16();
            br.ReadUInt16();
            m_newTargetGain = br.ReadSingle();
            m_onGain = br.ReadSingle();
            m_offGain = br.ReadSingle();
            m_isOk = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_targetWS);
            bw.WriteInt16(m_targetBoneIndex);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_newTargetGain);
            bw.WriteSingle(m_onGain);
            bw.WriteSingle(m_offGain);
            bw.WriteBoolean(m_isOk);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
