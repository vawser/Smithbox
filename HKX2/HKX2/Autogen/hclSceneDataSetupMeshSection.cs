using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclSceneDataSetupMeshSection : hkReferencedObject
    {
        public override uint Signature { get => 180466600; }
        
        public hclSceneDataSetupMesh m_setupMesh;
        public hkxMeshSection m_meshSection;
        public bool m_skinnedSection;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_setupMesh = des.ReadClassPointer<hclSceneDataSetupMesh>(br);
            m_meshSection = des.ReadClassPointer<hkxMeshSection>(br);
            m_skinnedSection = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteClassPointer<hclSceneDataSetupMesh>(bw, m_setupMesh);
            s.WriteClassPointer<hkxMeshSection>(bw, m_meshSection);
            bw.WriteBoolean(m_skinnedSection);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
