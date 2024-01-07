using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbStateMachineStateInfo : hkbBindable
    {
        public override uint Signature { get => 970417939; }
        
        public List<hkbStateListener> m_listeners;
        public hkbStateMachineEventPropertyArray m_enterNotifyEvents;
        public hkbStateMachineEventPropertyArray m_exitNotifyEvents;
        public hkbStateMachineTransitionInfoArray m_transitions;
        public hkbGenerator m_generator;
        public string m_name;
        public int m_stateId;
        public float m_probability;
        public bool m_enable;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_listeners = des.ReadClassPointerArray<hkbStateListener>(br);
            m_enterNotifyEvents = des.ReadClassPointer<hkbStateMachineEventPropertyArray>(br);
            m_exitNotifyEvents = des.ReadClassPointer<hkbStateMachineEventPropertyArray>(br);
            m_transitions = des.ReadClassPointer<hkbStateMachineTransitionInfoArray>(br);
            m_generator = des.ReadClassPointer<hkbGenerator>(br);
            m_name = des.ReadStringPointer(br);
            m_stateId = br.ReadInt32();
            m_probability = br.ReadSingle();
            m_enable = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkbStateListener>(bw, m_listeners);
            s.WriteClassPointer<hkbStateMachineEventPropertyArray>(bw, m_enterNotifyEvents);
            s.WriteClassPointer<hkbStateMachineEventPropertyArray>(bw, m_exitNotifyEvents);
            s.WriteClassPointer<hkbStateMachineTransitionInfoArray>(bw, m_transitions);
            s.WriteClassPointer<hkbGenerator>(bw, m_generator);
            s.WriteStringPointer(bw, m_name);
            bw.WriteInt32(m_stateId);
            bw.WriteSingle(m_probability);
            bw.WriteBoolean(m_enable);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
