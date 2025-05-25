using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclMeshMeshDeformSetupObject : hclOperatorSetupObject
    {
        public override uint Signature { get => 79408206; }
        
        public string m_name;
        public hclBufferSetupObject m_inputBufferSetupObject;
        public hclTriangleSelectionInput m_inputTriangleSelection;
        public hclBufferSetupObject m_outputBufferSetupObject;
        public hclVertexSelectionInput m_outputVertexSelection;
        public hclVertexFloatInput m_influenceRadiusPerVertex;
        public ScaleNormalBehaviour m_scaleNormalBehaviour;
        public uint m_maxTrianglesPerVertex;
        public float m_minimumTriangleWeight;
        public bool m_deformNormals;
        public bool m_deformTangents;
        public bool m_deformBiTangents;
        public bool m_useMeshTopology;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_inputBufferSetupObject = des.ReadClassPointer<hclBufferSetupObject>(br);
            m_inputTriangleSelection = new hclTriangleSelectionInput();
            m_inputTriangleSelection.Read(des, br);
            m_outputBufferSetupObject = des.ReadClassPointer<hclBufferSetupObject>(br);
            m_outputVertexSelection = new hclVertexSelectionInput();
            m_outputVertexSelection.Read(des, br);
            m_influenceRadiusPerVertex = new hclVertexFloatInput();
            m_influenceRadiusPerVertex.Read(des, br);
            m_scaleNormalBehaviour = (ScaleNormalBehaviour)br.ReadUInt32();
            m_maxTrianglesPerVertex = br.ReadUInt32();
            m_minimumTriangleWeight = br.ReadSingle();
            m_deformNormals = br.ReadBoolean();
            m_deformTangents = br.ReadBoolean();
            m_deformBiTangents = br.ReadBoolean();
            m_useMeshTopology = br.ReadBoolean();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointer<hclBufferSetupObject>(bw, m_inputBufferSetupObject);
            m_inputTriangleSelection.Write(s, bw);
            s.WriteClassPointer<hclBufferSetupObject>(bw, m_outputBufferSetupObject);
            m_outputVertexSelection.Write(s, bw);
            m_influenceRadiusPerVertex.Write(s, bw);
            bw.WriteUInt32((uint)m_scaleNormalBehaviour);
            bw.WriteUInt32(m_maxTrianglesPerVertex);
            bw.WriteSingle(m_minimumTriangleWeight);
            bw.WriteBoolean(m_deformNormals);
            bw.WriteBoolean(m_deformTangents);
            bw.WriteBoolean(m_deformBiTangents);
            bw.WriteBoolean(m_useMeshTopology);
        }
    }
}
