using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum StartStateMode
    {
        START_STATE_MODE_DEFAULT = 0,
        START_STATE_MODE_SYNC = 1,
        START_STATE_MODE_RANDOM = 2,
        START_STATE_MODE_CHOOSER = 3,
    }
    
    public enum StateMachineSelfTransitionMode
    {
        SELF_TRANSITION_MODE_NO_TRANSITION = 0,
        SELF_TRANSITION_MODE_TRANSITION_TO_START_STATE = 1,
        SELF_TRANSITION_MODE_FORCE_TRANSITION_TO_START_STATE = 2,
    }
    
    public partial class hkbStateMachine : hkbGenerator
    {
        public override uint Signature { get => 3777107537; }
        
        public hkbEvent m_eventToSendWhenStateOrTransitionChanges;
        public hkbCustomIdSelector m_startStateIdSelector;
        public int m_startStateId;
        public int m_returnToPreviousStateEventId;
        public int m_randomTransitionEventId;
        public int m_transitionToNextHigherStateEventId;
        public int m_transitionToNextLowerStateEventId;
        public int m_syncVariableIndex;
        public bool m_wrapAroundStateId;
        public sbyte m_maxSimultaneousTransitions;
        public StartStateMode m_startStateMode;
        public StateMachineSelfTransitionMode m_selfTransitionMode;
        public List<hkbStateMachineStateInfo> m_states;
        public hkbStateMachineTransitionInfoArray m_wildcardTransitions;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_eventToSendWhenStateOrTransitionChanges = new hkbEvent();
            m_eventToSendWhenStateOrTransitionChanges.Read(des, br);
            m_startStateIdSelector = des.ReadClassPointer<hkbCustomIdSelector>(br);
            m_startStateId = br.ReadInt32();
            m_returnToPreviousStateEventId = br.ReadInt32();
            m_randomTransitionEventId = br.ReadInt32();
            m_transitionToNextHigherStateEventId = br.ReadInt32();
            m_transitionToNextLowerStateEventId = br.ReadInt32();
            m_syncVariableIndex = br.ReadInt32();
            br.ReadUInt32();
            m_wrapAroundStateId = br.ReadBoolean();
            m_maxSimultaneousTransitions = br.ReadSByte();
            m_startStateMode = (StartStateMode)br.ReadSByte();
            m_selfTransitionMode = (StateMachineSelfTransitionMode)br.ReadSByte();
            br.ReadUInt64();
            m_states = des.ReadClassPointerArray<hkbStateMachineStateInfo>(br);
            m_wildcardTransitions = des.ReadClassPointer<hkbStateMachineTransitionInfoArray>(br);
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
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_eventToSendWhenStateOrTransitionChanges.Write(s, bw);
            s.WriteClassPointer<hkbCustomIdSelector>(bw, m_startStateIdSelector);
            bw.WriteInt32(m_startStateId);
            bw.WriteInt32(m_returnToPreviousStateEventId);
            bw.WriteInt32(m_randomTransitionEventId);
            bw.WriteInt32(m_transitionToNextHigherStateEventId);
            bw.WriteInt32(m_transitionToNextLowerStateEventId);
            bw.WriteInt32(m_syncVariableIndex);
            bw.WriteUInt32(0);
            bw.WriteBoolean(m_wrapAroundStateId);
            bw.WriteSByte(m_maxSimultaneousTransitions);
            bw.WriteSByte((sbyte)m_startStateMode);
            bw.WriteSByte((sbyte)m_selfTransitionMode);
            bw.WriteUInt64(0);
            s.WriteClassPointerArray<hkbStateMachineStateInfo>(bw, m_states);
            s.WriteClassPointer<hkbStateMachineTransitionInfoArray>(bw, m_wildcardTransitions);
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
        }
    }
}
