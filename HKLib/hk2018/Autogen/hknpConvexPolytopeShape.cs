// Automatically Generated

namespace HKLib.hk2018;

public class hknpConvexPolytopeShape : hknpConvexShape
{
    public List<Vector4> m_planes = new();

    public List<hknpConvexPolytopeShape.Face> m_faces = new();

    public List<byte> m_indices = new();

    public hknpConvexPolytopeShape.Connectivity? m_connectivity;


    public class Connectivity : hkReferencedObject
    {
        public List<hknpConvexPolytopeShape.Connectivity.Edge> m_vertexEdges = new();

        public List<hknpConvexPolytopeShape.Connectivity.Edge> m_faceLinks = new();


        public class Edge : IHavokObject
        {
            public ushort m_faceIndex;

            public byte m_edgeIndex;

        }


    }


    public class Face : IHavokObject
    {
        public ushort m_firstIndex;

        public byte m_numIndices;

        public byte m_minHalfAngle;

    }


}

