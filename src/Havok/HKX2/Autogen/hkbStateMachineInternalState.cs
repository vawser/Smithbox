using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbStateMachineInternalState : hkReferencedObject
    {
        public override uint Signature { get => 3054634337; }
        
        public List<hkbStateMachineActiveTransitionInfo> m_activeTransitions;
        public List<byte> m_transitionFlags;
        public List<byte> m_wildcardTransitionFlags;
        public List<hkbStateMachineDelayedTransitionInfo> m_delayedTransitions;
        public float m_timeInState;
        public float m_lastLocalTime;
        public int m_currentStateId;
        public int m_previousStateId;
        public int m_nextStartStateIndexOverride;
        public bool m_stateOrTransitionChanged;
        public bool m_echoNextUpdate;
        public bool m_hasEventlessWildcardTransitions;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_activeTransitions = des.ReadClassArray<hkbStateMachineActiveTransitionInfo>(br);
            m_transitionFlags = des.ReadByteArray(br);
            m_wildcardTransitionFlags = des.ReadByteArray(br);
            m_delayedTransitions = des.ReadClassArray<hkbStateMachineDelayedTransitionInfo>(br);
            m_timeInState = br.ReadSingle();
            m_lastLocalTime = br.ReadSingle();
            m_currentStateId = br.ReadInt32();
            m_previousStateId = br.ReadInt32();
            m_nextStartStateIndexOverride = br.ReadInt32();
            m_stateOrTransitionChanged = br.ReadBoolean();
            m_echoNextUpdate = br.ReadBoolean();
            m_hasEventlessWildcardTransitions = br.ReadBoolean();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbStateMachineActiveTransitionInfo>(bw, m_activeTransitions);
            s.WriteByteArray(bw, m_transitionFlags);
            s.WriteByteArray(bw, m_wildcardTransitionFlags);
            s.WriteClassArray<hkbStateMachineDelayedTransitionInfo>(bw, m_delayedTransitions);
            bw.WriteSingle(m_timeInState);
            bw.WriteSingle(m_lastLocalTime);
            bw.WriteInt32(m_currentStateId);
            bw.WriteInt32(m_previousStateId);
            bw.WriteInt32(m_nextStartStateIndexOverride);
            bw.WriteBoolean(m_stateOrTransitionChanged);
            bw.WriteBoolean(m_echoNextUpdate);
            bw.WriteBoolean(m_hasEventlessWildcardTransitions);
            bw.WriteByte(0);
        }
    }
}
