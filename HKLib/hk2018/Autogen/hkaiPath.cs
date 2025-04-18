// Automatically Generated

namespace HKLib.hk2018;

public class hkaiPath : hkReferencedObject
{
    public List<hkaiPath.PathPoint> m_points = new();

    public hkaiPath.ReferenceFrame m_referenceFrame;


    public enum ReferenceFrame : int
    {
        REFERENCE_FRAME_WORLD = 0,
        REFERENCE_FRAME_SECTION_LOCAL = 1,
        REFERENCE_FRAME_SECTION_FIXED = 2
    }

    [Flags]
    public enum PathPointBits : int
    {
        EDGE_TYPE_USER_START = 1,
        EDGE_TYPE_USER_END = 2,
        EDGE_TYPE_SEGMENT_START = 4,
        EDGE_TYPE_SEGMENT_END = 8
    }

    public class PathPoint : IHavokObject
    {
        public Vector4 m_position = new();

        public Vector4 m_normal = new();

        public uint m_userEdgeData;

        public int m_sectionId;

        public hkaiPath.PathPointBits m_flags;

    }


}

