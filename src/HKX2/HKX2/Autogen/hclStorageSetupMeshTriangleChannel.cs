using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclStorageSetupMeshTriangleChannel : IHavokObject
    {
        public virtual uint Signature { get => 1935860136; }
        
        public string m_name;
        public TriangleChannelType m_type;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_name = des.ReadStringPointer(br);
            m_type = (TriangleChannelType)br.ReadUInt32();
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
