using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ExpressionEventMode
    {
        EVENT_MODE_SEND_ONCE = 0,
        EVENT_MODE_SEND_ON_TRUE = 1,
        EVENT_MODE_SEND_ON_FALSE_TO_TRUE = 2,
        EVENT_MODE_SEND_EVERY_FRAME_ONCE_TRUE = 3,
    }
    
    public partial class hkbExpressionData : IHavokObject
    {
        public virtual uint Signature { get => 1732248618; }
        
        public string m_expression;
        public int m_assignmentVariableIndex;
        public int m_assignmentEventIndex;
        public ExpressionEventMode m_eventMode;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_expression = des.ReadStringPointer(br);
            m_assignmentVariableIndex = br.ReadInt32();
            m_assignmentEventIndex = br.ReadInt32();
            m_eventMode = (ExpressionEventMode)br.ReadSByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_expression);
            bw.WriteInt32(m_assignmentVariableIndex);
            bw.WriteInt32(m_assignmentEventIndex);
            bw.WriteSByte((sbyte)m_eventMode);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
