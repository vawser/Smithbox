using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclScratchBufferDefinition : hclBufferDefinition
    {
        public override uint Signature { get => 2685602348; }
        
        public List<ushort> m_triangleIndices;
        public bool m_storeNormals;
        public bool m_storeTangentsAndBiTangents;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_triangleIndices = des.ReadUInt16Array(br);
            m_storeNormals = br.ReadBoolean();
            m_storeTangentsAndBiTangents = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteUInt16Array(bw, m_triangleIndices);
            bw.WriteBoolean(m_storeNormals);
            bw.WriteBoolean(m_storeTangentsAndBiTangents);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
