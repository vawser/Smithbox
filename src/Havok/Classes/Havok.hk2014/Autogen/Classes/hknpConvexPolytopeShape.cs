// Automatically Generated

namespace Havok.hk2014;

public partial class hknpConvexPolytopeShape : hknpConvexShape
{
    public List<Vector4> m_planes = new();

    public List<hknpConvexPolytopeShape.Face> m_faces = new();

    public List<byte> m_indices = new();


    public class Face : IHavokObject
    {
        public ushort m_firstIndex;

        public byte m_numIndices;

        public byte m_minHalfAngle;

    }

}
