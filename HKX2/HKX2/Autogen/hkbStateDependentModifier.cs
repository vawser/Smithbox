using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbStateDependentModifier : hkbModifier
    {
        public override uint Signature { get => 39268832; }
        
        public bool m_applyModifierDuringTransition;
        public List<int> m_stateIds;
        public hkbModifier m_modifier;
        public bool m_isActive;
        public hkbStateMachine m_stateMachine;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_applyModifierDuringTransition = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_stateIds = des.ReadInt32Array(br);
            m_modifier = des.ReadClassPointer<hkbModifier>(br);
            m_isActive = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_stateMachine = des.ReadClassPointer<hkbStateMachine>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteBoolean(m_applyModifierDuringTransition);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteInt32Array(bw, m_stateIds);
            s.WriteClassPointer<hkbModifier>(bw, m_modifier);
            bw.WriteBoolean(m_isActive);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteClassPointer<hkbStateMachine>(bw, m_stateMachine);
        }
    }
}
