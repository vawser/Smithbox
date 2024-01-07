using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBendLinkSetupObject : hclConstraintSetSetupObject
    {
        public override uint Signature { get => 2979266589; }
        
        public string m_name;
        public hclSimulationSetupMesh m_simulationMesh;
        public bool m_createStandardLinks;
        public hclVertexSelectionInput m_vertexSelection;
        public hclVertexFloatInput m_bendStiffness;
        public hclVertexFloatInput m_stretchStiffness;
        public hclVertexFloatInput m_flatnessFactor;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_simulationMesh = des.ReadClassPointer<hclSimulationSetupMesh>(br);
            m_createStandardLinks = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_vertexSelection = new hclVertexSelectionInput();
            m_vertexSelection.Read(des, br);
            m_bendStiffness = new hclVertexFloatInput();
            m_bendStiffness.Read(des, br);
            m_stretchStiffness = new hclVertexFloatInput();
            m_stretchStiffness.Read(des, br);
            m_flatnessFactor = new hclVertexFloatInput();
            m_flatnessFactor.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointer<hclSimulationSetupMesh>(bw, m_simulationMesh);
            bw.WriteBoolean(m_createStandardLinks);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            m_vertexSelection.Write(s, bw);
            m_bendStiffness.Write(s, bw);
            m_stretchStiffness.Write(s, bw);
            m_flatnessFactor.Write(s, bw);
        }
    }
}
