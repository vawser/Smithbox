using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbStateMachineDelayedTransitionInfo : IHavokObject
    {
        public virtual uint Signature { get => 40719513; }
        
        public hkbStateMachineProspectiveTransitionInfo m_delayedTransition;
        public float m_timeDelayed;
        public bool m_isDelayedTransitionReturnToPreviousState;
        public bool m_wasInAbutRangeLastFrame;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_delayedTransition = new hkbStateMachineProspectiveTransitionInfo();
            m_delayedTransition.Read(des, br);
            m_timeDelayed = br.ReadSingle();
            m_isDelayedTransitionReturnToPreviousState = br.ReadBoolean();
            m_wasInAbutRangeLastFrame = br.ReadBoolean();
            br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_delayedTransition.Write(s, bw);
            bw.WriteSingle(m_timeDelayed);
            bw.WriteBoolean(m_isDelayedTransitionReturnToPreviousState);
            bw.WriteBoolean(m_wasInAbutRangeLastFrame);
            bw.WriteUInt16(0);
        }
    }
}
