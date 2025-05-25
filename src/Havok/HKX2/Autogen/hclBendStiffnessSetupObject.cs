using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBendStiffnessSetupObject : hclConstraintSetSetupObject
    {
        public override uint Signature { get => 1524085618; }
        
        public string m_name;
        public hclSimulationSetupMesh m_simulationMesh;
        public hclVertexSelectionInput m_vertexSelection;
        public hclVertexFloatInput m_bendStiffness;
        public bool m_useRestPoseConfig;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_simulationMesh = des.ReadClassPointer<hclSimulationSetupMesh>(br);
            m_vertexSelection = new hclVertexSelectionInput();
            m_vertexSelection.Read(des, br);
            m_bendStiffness = new hclVertexFloatInput();
            m_bendStiffness.Read(des, br);
            m_useRestPoseConfig = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointer<hclSimulationSetupMesh>(bw, m_simulationMesh);
            m_vertexSelection.Write(s, bw);
            m_bendStiffness.Write(s, bw);
            bw.WriteBoolean(m_useRestPoseConfig);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
