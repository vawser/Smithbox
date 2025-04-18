using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum SelfTransitionMode
    {
        SELF_TRANSITION_MODE_CONTINUE_IF_CYCLIC_BLEND_IF_ACYCLIC = 0,
        SELF_TRANSITION_MODE_CONTINUE = 1,
        SELF_TRANSITION_MODE_RESET = 2,
        SELF_TRANSITION_MODE_BLEND = 3,
    }
    
    public enum EventMode
    {
        EVENT_MODE_DEFAULT = 0,
        EVENT_MODE_PROCESS_ALL = 1,
        EVENT_MODE_IGNORE_FROM_GENERATOR = 2,
        EVENT_MODE_IGNORE_TO_GENERATOR = 3,
    }
    
    public partial class hkbTransitionEffect : hkbGenerator
    {
        public override uint Signature { get => 3970553188; }
        
        public SelfTransitionMode m_selfTransitionMode;
        public EventMode m_eventMode;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_selfTransitionMode = (SelfTransitionMode)br.ReadSByte();
            m_eventMode = (EventMode)br.ReadSByte();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSByte((sbyte)m_selfTransitionMode);
            bw.WriteSByte((sbyte)m_eventMode);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
