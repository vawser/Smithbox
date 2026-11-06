// Automatically Generated

namespace Havok.hk2019.hkaiNavMeshGeneration;

public class InputVolume : hkReferencedObject
{
    public List<hkaiNavMeshGeneration.InputVolume.Face> m_faces = new();

    public List<hkaiNavMeshGeneration.InputVolume.Edge> m_edges = new();

    public List<hkFpMath.FpIntVec3_4> m_vertices = new();


    public class Edge : IHavokObject
    {
        public int m_faceIndex;

        public int m_vertexIndex;

        public int m_prevEdgeIndex;

        public int m_nextEdgeIndex;

        public int m_oppositeEdgeIndex;

    }

    public class Face : IHavokObject
    {
        public int m_edgeIndex;

        public hkaiClipper.BiplanarInnerTypes.ImplicitPlane m_plane = new();

    }

}
