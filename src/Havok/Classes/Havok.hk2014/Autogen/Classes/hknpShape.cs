// Automatically Generated

namespace Havok.hk2014;

public class hknpShape : hkReferencedObject
{
    public hknpShape.FlagsEnum m_flags;

    public byte m_numShapeKeyBits;

    public byte m_dispatchType;

    public float m_convexRadius;

    public ulong m_userData;

    public hkRefCountedProperties? m_properties;


    [Flags]
    public enum FlagsEnum : int
    {
        IS_CONVEX_SHAPE = 1,
        IS_CONVEX_POLYTOPE_SHAPE = 2,
        IS_COMPOSITE_SHAPE = 4,
        IS_HEIGHT_FIELD_SHAPE = 8,
        USE_SINGLE_POINT_MANIFOLD = 16,
        IS_INTERIOR_TRIANGLE = 32,
        SUPPORTS_COLLISIONS_WITH_INTERIOR_TRIANGLES = 64,
        USE_NORMAL_TO_FIND_SUPPORT_PLANE = 128,
        USE_SMALL_FACE_INDICES = 256,
        NO_GET_SHAPE_KEYS_ON_SPU = 512,
        SHAPE_NOT_SUPPORTED_ON_SPU = 1024,
        IS_TRIANGLE_OR_QUAD_SHAPE = 2048,
        IS_QUAD_SHAPE = 4096
    }

}
