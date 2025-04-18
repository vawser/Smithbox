using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum VertexSelectionMethod
    {
        PROPORTIONAL_TO_AREA = 0,
        PROPORTIONAL_TO_VERTICES = 1,
    }
    
    public partial class hkaiNavMeshSimplificationUtilsExtraVertexSettings : IHavokObject
    {
        public virtual uint Signature { get => 3836726061; }
        
        public VertexSelectionMethod m_vertexSelectionMethod;
        public float m_vertexFraction;
        public float m_areaFraction;
        public float m_minPartitionArea;
        public int m_numSmoothingIterations;
        public float m_iterationDamping;
        public bool m_addVerticesOnBoundaryEdges;
        public bool m_addVerticesOnPartitionBorders;
        public float m_boundaryEdgeSplitLength;
        public float m_partitionBordersSplitLength;
        public float m_userVertexOnBoundaryTolerance;
        public List<Vector4> m_userVertices;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_vertexSelectionMethod = (VertexSelectionMethod)br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
            m_vertexFraction = br.ReadSingle();
            m_areaFraction = br.ReadSingle();
            m_minPartitionArea = br.ReadSingle();
            m_numSmoothingIterations = br.ReadInt32();
            m_iterationDamping = br.ReadSingle();
            m_addVerticesOnBoundaryEdges = br.ReadBoolean();
            m_addVerticesOnPartitionBorders = br.ReadBoolean();
            br.ReadUInt16();
            m_boundaryEdgeSplitLength = br.ReadSingle();
            m_partitionBordersSplitLength = br.ReadSingle();
            m_userVertexOnBoundaryTolerance = br.ReadSingle();
            m_userVertices = des.ReadVector4Array(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteByte((byte)m_vertexSelectionMethod);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_vertexFraction);
            bw.WriteSingle(m_areaFraction);
            bw.WriteSingle(m_minPartitionArea);
            bw.WriteInt32(m_numSmoothingIterations);
            bw.WriteSingle(m_iterationDamping);
            bw.WriteBoolean(m_addVerticesOnBoundaryEdges);
            bw.WriteBoolean(m_addVerticesOnPartitionBorders);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_boundaryEdgeSplitLength);
            bw.WriteSingle(m_partitionBordersSplitLength);
            bw.WriteSingle(m_userVertexOnBoundaryTolerance);
            s.WriteVector4Array(bw, m_userVertices);
        }
    }
}
