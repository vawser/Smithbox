using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclMeshMeshDeformOperator : hclOperator
    {
        public override uint Signature { get => 4283377091; }
        
        public enum ScaleNormalBehaviour
        {
            SCALE_NORMAL_IGNORE = 0,
            SCALE_NORMAL_APPLY = 1,
            SCALE_NORMAL_INVERT = 2,
        }
        
        public List<ushort> m_inputTrianglesSubset;
        public List<hclMeshMeshDeformOperatorTriangleVertexPair> m_triangleVertexPairs;
        public List<ushort> m_triangleVertexStartForVertex;
        public uint m_inputBufferIdx;
        public uint m_outputBufferIdx;
        public ushort m_startVertex;
        public ushort m_endVertex;
        public ScaleNormalBehaviour m_scaleNormalBehaviour;
        public bool m_deformNormals;
        public bool m_partialDeform;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_inputTrianglesSubset = des.ReadUInt16Array(br);
            m_triangleVertexPairs = des.ReadClassArray<hclMeshMeshDeformOperatorTriangleVertexPair>(br);
            m_triangleVertexStartForVertex = des.ReadUInt16Array(br);
            m_inputBufferIdx = br.ReadUInt32();
            m_outputBufferIdx = br.ReadUInt32();
            m_startVertex = br.ReadUInt16();
            m_endVertex = br.ReadUInt16();
            m_scaleNormalBehaviour = (ScaleNormalBehaviour)br.ReadUInt32();
            m_deformNormals = br.ReadBoolean();
            m_partialDeform = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteUInt16Array(bw, m_inputTrianglesSubset);
            s.WriteClassArray<hclMeshMeshDeformOperatorTriangleVertexPair>(bw, m_triangleVertexPairs);
            s.WriteUInt16Array(bw, m_triangleVertexStartForVertex);
            bw.WriteUInt32(m_inputBufferIdx);
            bw.WriteUInt32(m_outputBufferIdx);
            bw.WriteUInt16(m_startVertex);
            bw.WriteUInt16(m_endVertex);
            bw.WriteUInt32((uint)m_scaleNormalBehaviour);
            bw.WriteBoolean(m_deformNormals);
            bw.WriteBoolean(m_partialDeform);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
