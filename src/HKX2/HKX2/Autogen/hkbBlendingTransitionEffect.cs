using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum EndMode
    {
        END_MODE_NONE = 0,
        END_MODE_TRANSITION_UNTIL_END_OF_FROM_GENERATOR = 1,
        END_MODE_CAP_DURATION_AT_END_OF_FROM_GENERATOR = 2,
    }
    
    public partial class hkbBlendingTransitionEffect : hkbTransitionEffect
    {
        public override uint Signature { get => 350571612; }
        
        public enum FlagBits
        {
            FLAG_NONE = 0,
            FLAG_IGNORE_FROM_WORLD_FROM_MODEL = 1,
            FLAG_SYNC = 2,
            FLAG_IGNORE_TO_WORLD_FROM_MODEL = 4,
            FLAG_IGNORE_TO_WORLD_FROM_MODEL_ROTATION = 8,
        }
        
        public float m_duration;
        public float m_toGeneratorStartTimeFraction;
        public ushort m_flags;
        public EndMode m_endMode;
        public BlendCurve m_blendCurve;
        public short m_alignmentBone;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_duration = br.ReadSingle();
            m_toGeneratorStartTimeFraction = br.ReadSingle();
            m_flags = br.ReadUInt16();
            m_endMode = (EndMode)br.ReadSByte();
            m_blendCurve = (BlendCurve)br.ReadSByte();
            m_alignmentBone = br.ReadInt16();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_duration);
            bw.WriteSingle(m_toGeneratorStartTimeFraction);
            bw.WriteUInt16(m_flags);
            bw.WriteSByte((sbyte)m_endMode);
            bw.WriteSByte((sbyte)m_blendCurve);
            bw.WriteInt16(m_alignmentBone);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
        }
    }
}
