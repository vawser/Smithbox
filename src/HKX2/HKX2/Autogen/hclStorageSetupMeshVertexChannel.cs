using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclStorageSetupMeshVertexChannel : IHavokObject
    {
        public virtual uint Signature { get => 2365380327; }
        
        public string m_name;
        public VertexChannelType m_type;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_name = des.ReadStringPointer(br);
            m_type = (VertexChannelType)br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_name);
            bw.WriteUInt32((uint)m_type);
            bw.WriteUInt32(0);
        }
    }
}
