using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbStateMachineActiveTransitionInfo : IHavokObject
    {
        public virtual uint Signature { get => 3146831183; }
        
        public hkbNodeInternalStateInfo m_transitionEffectInternalStateInfo;
        public hkbStateMachineTransitionInfoReference m_transitionInfoReference;
        public hkbStateMachineTransitionInfoReference m_transitionInfoReferenceForTE;
        public int m_fromStateId;
        public int m_toStateId;
        public bool m_isReturnToPreviousState;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            br.ReadUInt64();
            m_transitionEffectInternalStateInfo = des.ReadClassPointer<hkbNodeInternalStateInfo>(br);
            m_transitionInfoReference = new hkbStateMachineTransitionInfoReference();
            m_transitionInfoReference.Read(des, br);
            m_transitionInfoReferenceForTE = new hkbStateMachineTransitionInfoReference();
            m_transitionInfoReferenceForTE.Read(des, br);
            m_fromStateId = br.ReadInt32();
            m_toStateId = br.ReadInt32();
            m_isReturnToPreviousState = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt64(0);
            s.WriteClassPointer<hkbNodeInternalStateInfo>(bw, m_transitionEffectInternalStateInfo);
            m_transitionInfoReference.Write(s, bw);
            m_transitionInfoReferenceForTE.Write(s, bw);
            bw.WriteInt32(m_fromStateId);
            bw.WriteInt32(m_toStateId);
            bw.WriteBoolean(m_isReturnToPreviousState);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
