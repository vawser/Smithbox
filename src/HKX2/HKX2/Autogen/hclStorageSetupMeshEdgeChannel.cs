using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclStorageSetupMeshEdgeChannel : IHavokObject
    {
        public virtual uint Signature { get => 4187433602; }
        
        public string m_name;
        public EdgeChannelType m_type;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_name = des.ReadStringPointer(br);
            m_type = (EdgeChannelType)br.ReadUInt32();
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
