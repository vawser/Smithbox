using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbGeneratorTransitionEffectInternalState : hkReferencedObject
    {
        public override uint Signature { get => 3943327924; }
        
        public float m_timeInTransition;
        public float m_duration;
        public float m_effectiveBlendInDuration;
        public float m_effectiveBlendOutDuration;
        public ToGeneratorState m_toGeneratorState;
        public bool m_echoTransitionGenerator;
        public SelfTransitionMode m_toGeneratorSelfTransitionMode;
        public bool m_justActivated;
        public bool m_updateActiveNodes;
        public Stage m_stage;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_timeInTransition = br.ReadSingle();
            m_duration = br.ReadSingle();
            m_effectiveBlendInDuration = br.ReadSingle();
            m_effectiveBlendOutDuration = br.ReadSingle();
            m_toGeneratorState = (ToGeneratorState)br.ReadSByte();
            m_echoTransitionGenerator = br.ReadBoolean();
            m_toGeneratorSelfTransitionMode = (SelfTransitionMode)br.ReadSByte();
            m_justActivated = br.ReadBoolean();
            m_updateActiveNodes = br.ReadBoolean();
            m_stage = (Stage)br.ReadSByte();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_timeInTransition);
            bw.WriteSingle(m_duration);
            bw.WriteSingle(m_effectiveBlendInDuration);
            bw.WriteSingle(m_effectiveBlendOutDuration);
            bw.WriteSByte((sbyte)m_toGeneratorState);
            bw.WriteBoolean(m_echoTransitionGenerator);
            bw.WriteSByte((sbyte)m_toGeneratorSelfTransitionMode);
            bw.WriteBoolean(m_justActivated);
            bw.WriteBoolean(m_updateActiveNodes);
            bw.WriteSByte((sbyte)m_stage);
            bw.WriteUInt16(0);
        }
    }
}
