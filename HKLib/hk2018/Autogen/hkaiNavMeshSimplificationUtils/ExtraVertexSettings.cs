// Automatically Generated

namespace HKLib.hk2018.hkaiNavMeshSimplificationUtils;

public class ExtraVertexSettings : IHavokObject
{
    public hkaiNavMeshSimplificationUtils.ExtraVertexSettings.VertexSelectionMethod m_vertexSelectionMethod;

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

    public List<Vector4> m_userVertices = new();


    public enum VertexSelectionMethod : int
    {
        PROPORTIONAL_TO_AREA = 0,
        PROPORTIONAL_TO_VERTICES = 1
    }

}

