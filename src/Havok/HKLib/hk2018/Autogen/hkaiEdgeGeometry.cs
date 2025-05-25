// Automatically Generated

namespace HKLib.hk2018;

public class hkaiEdgeGeometry : hkReferencedObject
{
    public List<hkaiEdgeGeometry.Edge> m_edges = new();

    public List<hkaiEdgeGeometry.Face> m_faces = new();

    public List<Vector4> m_vertices = new();

    public hkaiEdgeGeometry.Face m_zeroFace = new();


    [Flags]
    public enum FaceFlagBits : int
    {
        FLAGS_NONE = 0,
        FLAGS_INPUT_GEOMETRY = 1,
        FLAGS_PAINTER = 2,
        FLAGS_CARVER = 4,
        FLAGS_EXTRUDED = 8,
        FLAGS_ALL = 15
    }

    public class Face : IHavokObject
    {
        public uint m_data;

        public uint m_faceIndex;

        public hkaiEdgeGeometry.FaceFlagBits m_flags;

    }


    public class Edge : IHavokObject
    {
        public uint m_a;

        public uint m_b;

        public uint m_face;

        public uint m_data;

    }


}

