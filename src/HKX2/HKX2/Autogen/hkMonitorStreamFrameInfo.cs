using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum AbsoluteTimeCounter
    {
        ABSOLUTE_TIME_TIMER_0 = 0,
        ABSOLUTE_TIME_TIMER_1 = 1,
        ABSOLUTE_TIME_NOT_TIMED = -1,
    }
    
    public partial class hkMonitorStreamFrameInfo : IHavokObject
    {
        public virtual uint Signature { get => 2006497243; }
        
        public string m_heading;
        public int m_indexOfTimer0;
        public int m_indexOfTimer1;
        public AbsoluteTimeCounter m_absoluteTimeCounter;
        public float m_timerFactor0;
        public float m_timerFactor1;
        public int m_threadId;
        public int m_frameStreamStart;
        public int m_frameStreamEnd;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_heading = des.ReadStringPointer(br);
            m_indexOfTimer0 = br.ReadInt32();
            m_indexOfTimer1 = br.ReadInt32();
            m_absoluteTimeCounter = (AbsoluteTimeCounter)br.ReadUInt32();
            m_timerFactor0 = br.ReadSingle();
            m_timerFactor1 = br.ReadSingle();
            m_threadId = br.ReadInt32();
            m_frameStreamStart = br.ReadInt32();
            m_frameStreamEnd = br.ReadInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_heading);
            bw.WriteInt32(m_indexOfTimer0);
            bw.WriteInt32(m_indexOfTimer1);
            bw.WriteUInt32((uint)m_absoluteTimeCounter);
            bw.WriteSingle(m_timerFactor0);
            bw.WriteSingle(m_timerFactor1);
            bw.WriteInt32(m_threadId);
            bw.WriteInt32(m_frameStreamStart);
            bw.WriteInt32(m_frameStreamEnd);
        }
    }
}
