using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum VertexSelectionType
    {
        VERTEX_SELECTION_ALL = 0,
        VERTEX_SELECTION_NONE = 1,
        VERTEX_SELECTION_CHANNEL = 2,
        VERTEX_SELECTION_INVERSE_CHANNEL = 3,
    }
    
    public partial class hclVertexSelectionInput : IHavokObject
    {
        public virtual uint Signature { get => 1167178747; }
        
        public VertexSelectionType m_type;
        public string m_channelName;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_type = (VertexSelectionType)br.ReadUInt32();
            br.ReadUInt32();
            m_channelName = des.ReadStringPointer(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32((uint)m_type);
            bw.WriteUInt32(0);
            s.WriteStringPointer(bw, m_channelName);
        }
    }
}
