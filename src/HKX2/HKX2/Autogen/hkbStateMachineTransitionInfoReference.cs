using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbStateMachineTransitionInfoReference : IHavokObject
    {
        public virtual uint Signature { get => 2551235280; }
        
        public short m_fromStateIndex;
        public short m_transitionIndex;
        public short m_stateMachineId;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_fromStateIndex = br.ReadInt16();
            m_transitionIndex = br.ReadInt16();
            m_stateMachineId = br.ReadInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt16(m_fromStateIndex);
            bw.WriteInt16(m_transitionIndex);
            bw.WriteInt16(m_stateMachineId);
        }
    }
}
