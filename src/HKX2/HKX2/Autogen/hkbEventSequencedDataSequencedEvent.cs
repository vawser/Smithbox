using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbEventSequencedDataSequencedEvent : IHavokObject
    {
        public virtual uint Signature { get => 2436479009; }
        
        public hkbEvent m_event;
        public float m_time;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_event = new hkbEvent();
            m_event.Read(des, br);
            m_time = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_event.Write(s, bw);
            bw.WriteSingle(m_time);
            bw.WriteUInt32(0);
        }
    }
}
