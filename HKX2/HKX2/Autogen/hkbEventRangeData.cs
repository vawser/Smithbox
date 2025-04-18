using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum EventRangeMode
    {
        EVENT_MODE_SEND_ON_ENTER_RANGE = 0,
        EVENT_MODE_SEND_WHEN_IN_RANGE = 1,
    }
    
    public partial class hkbEventRangeData : IHavokObject
    {
        public virtual uint Signature { get => 1824074870; }
        
        public float m_upperBound;
        public hkbEventProperty m_event;
        public EventRangeMode m_eventMode;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_upperBound = br.ReadSingle();
            br.ReadUInt32();
            m_event = new hkbEventProperty();
            m_event.Read(des, br);
            m_eventMode = (EventRangeMode)br.ReadSByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_upperBound);
            bw.WriteUInt32(0);
            m_event.Write(s, bw);
            bw.WriteSByte((sbyte)m_eventMode);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
