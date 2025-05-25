using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbStateMachineTimeInterval : IHavokObject
    {
        public virtual uint Signature { get => 1621656037; }
        
        public int m_enterEventId;
        public int m_exitEventId;
        public float m_enterTime;
        public float m_exitTime;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_enterEventId = br.ReadInt32();
            m_exitEventId = br.ReadInt32();
            m_enterTime = br.ReadSingle();
            m_exitTime = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_enterEventId);
            bw.WriteInt32(m_exitEventId);
            bw.WriteSingle(m_enterTime);
            bw.WriteSingle(m_exitTime);
        }
    }
}
