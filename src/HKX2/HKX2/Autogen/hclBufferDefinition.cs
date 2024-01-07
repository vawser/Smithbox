using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBufferDefinition : hkReferencedObject
    {
        public override uint Signature { get => 2135579644; }
        
        public string m_name;
        public int m_type;
        public int m_subType;
        public uint m_numVertices;
        public uint m_numTriangles;
        public hclBufferLayout m_bufferLayout;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_type = br.ReadInt32();
            m_subType = br.ReadInt32();
            m_numVertices = br.ReadUInt32();
            m_numTriangles = br.ReadUInt32();
            m_bufferLayout = new hclBufferLayout();
            m_bufferLayout.Read(des, br);
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            bw.WriteInt32(m_type);
            bw.WriteInt32(m_subType);
            bw.WriteUInt32(m_numVertices);
            bw.WriteUInt32(m_numTriangles);
            m_bufferLayout.Write(s, bw);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
