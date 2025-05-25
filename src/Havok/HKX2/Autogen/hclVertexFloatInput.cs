using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum VertexFloatType
    {
        VERTEX_FLOAT_CONSTANT = 0,
        VERTEX_FLOAT_CHANNEL = 1,
    }
    
    public partial class hclVertexFloatInput : IHavokObject
    {
        public virtual uint Signature { get => 4150435535; }
        
        public VertexFloatType m_type;
        public float m_constantValue;
        public string m_channelName;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_type = (VertexFloatType)br.ReadUInt32();
            m_constantValue = br.ReadSingle();
            m_channelName = des.ReadStringPointer(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32((uint)m_type);
            bw.WriteSingle(m_constantValue);
            s.WriteStringPointer(bw, m_channelName);
        }
    }
}
