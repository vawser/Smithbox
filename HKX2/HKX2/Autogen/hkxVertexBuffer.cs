using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxVertexBuffer : hkReferencedObject
    {
        public override uint Signature { get => 3340843844; }
        
        public hkxVertexBufferVertexData m_data;
        public hkxVertexDescription m_desc;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_data = new hkxVertexBufferVertexData();
            m_data.Read(des, br);
            m_desc = new hkxVertexDescription();
            m_desc.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_data.Write(s, bw);
            m_desc.Write(s, bw);
        }
    }
}
