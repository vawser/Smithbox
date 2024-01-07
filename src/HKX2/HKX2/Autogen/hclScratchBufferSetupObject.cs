using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclScratchBufferSetupObject : hclBufferSetupObject
    {
        public override uint Signature { get => 2025773992; }
        
        public string m_name;
        public hclSetupMesh m_setupMesh;
        public bool m_storeNormals;
        public bool m_storeTangentsAndBiTangents;
        public bool m_storeTriangles;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_setupMesh = des.ReadClassPointer<hclSetupMesh>(br);
            m_storeNormals = br.ReadBoolean();
            m_storeTangentsAndBiTangents = br.ReadBoolean();
            m_storeTriangles = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointer<hclSetupMesh>(bw, m_setupMesh);
            bw.WriteBoolean(m_storeNormals);
            bw.WriteBoolean(m_storeTangentsAndBiTangents);
            bw.WriteBoolean(m_storeTriangles);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
        }
    }
}
