using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ToGeneratorState
    {
        STATE_INACTIVE = 0,
        STATE_READY_FOR_SET_LOCAL_TIME = 1,
        STATE_READY_FOR_APPLY_SELF_TRANSITION_MODE = 2,
        STATE_ACTIVE = 3,
    }
    
    public enum Stage
    {
        STAGE_BLENDING_IN = 0,
        STAGE_PLAYING_TRANSITION_GENERATOR = 1,
        STAGE_BLENDING_OUT = 2,
    }
    
    public enum ChildState
    {
        CHILD_FROM_GENERATOR = 0,
        CHILD_TRANSITION_GENERATOR = 1,
        CHILD_TO_GENERATOR = 2,
        CHILD_NONE = 3,
    }
    
    public partial class hkbGeneratorTransitionEffect : hkbTransitionEffect
    {
        public override uint Signature { get => 1142984455; }
        
        public hkbGenerator m_transitionGenerator;
        public float m_blendInDuration;
        public float m_blendOutDuration;
        public bool m_syncToGeneratorStartTime;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_transitionGenerator = des.ReadClassPointer<hkbGenerator>(br);
            m_blendInDuration = br.ReadSingle();
            m_blendOutDuration = br.ReadSingle();
            m_syncToGeneratorStartTime = br.ReadBoolean();
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
            s.WriteClassPointer<hkbGenerator>(bw, m_transitionGenerator);
            bw.WriteSingle(m_blendInDuration);
            bw.WriteSingle(m_blendOutDuration);
            bw.WriteBoolean(m_syncToGeneratorStartTime);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
