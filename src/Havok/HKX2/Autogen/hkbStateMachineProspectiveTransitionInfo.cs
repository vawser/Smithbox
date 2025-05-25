using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbStateMachineProspectiveTransitionInfo : IHavokObject
    {
        public virtual uint Signature { get => 984652334; }
        
        public hkbStateMachineTransitionInfoReference m_transitionInfoReference;
        public hkbStateMachineTransitionInfoReference m_transitionInfoReferenceForTE;
        public int m_toStateId;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_transitionInfoReference = new hkbStateMachineTransitionInfoReference();
            m_transitionInfoReference.Read(des, br);
            m_transitionInfoReferenceForTE = new hkbStateMachineTransitionInfoReference();
            m_transitionInfoReferenceForTE.Read(des, br);
            m_toStateId = br.ReadInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_transitionInfoReference.Write(s, bw);
            m_transitionInfoReferenceForTE.Write(s, bw);
            bw.WriteInt32(m_toStateId);
        }
    }
}
