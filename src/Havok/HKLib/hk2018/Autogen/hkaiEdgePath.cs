// Automatically Generated

namespace HKLib.hk2018;

public class hkaiEdgePath : hkReferencedObject
{
    public List<hkaiEdgePath.Edge> m_edges = new();

    public List<int> m_edgeData = new();

    public int m_edgeDataStriding;

    public float m_leftTurnRadius;

    public float m_rightTurnRadius;

    public float m_characterRadius;


    public class TraversalState : IHavokObject
    {
        public int m_faceEdge;

        public int m_trailingEdge;

        public int m_highestUserEdgeCrossed;

    }


    public class Edge : IHavokObject
    {
        public Vector4 m_left = new();

        public Vector4 m_right = new();

        public Vector4 m_up = new();

        public Matrix4x4 m_followingTransform = new();

        public hkaiPersistentEdgeKey m_edge = new();

        public hkaiEdgePath.FollowingCornerInfo m_leftFollowingCorner = new();

        public hkaiEdgePath.FollowingCornerInfo m_rightFollowingCorner = new();

        public hkaiNavMesh.EdgeFlagBits m_flags;

    }


    public class FollowingCornerInfo : IHavokObject
    {
        public ushort m_data;

    }


}

