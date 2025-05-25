// Automatically Generated

namespace HKLib.hk2018;

public class hknpShape : hkReferencedObject
{
    public hknpShape.FlagsEnum m_flags;

    public hknpShapeType.Enum m_type;

    public byte m_numShapeKeyBits;

    public hknpCollisionDispatchType.Enum m_dispatchType;

    public float m_convexRadius;

    public ulong m_userData;

    public hkRefCountedProperties? m_properties;


    public enum ConvexRadiusDisplayMode : int
    {
        CONVEX_RADIUS_DISPLAY_NONE = 0,
        CONVEX_RADIUS_DISPLAY_PLANAR = 1,
        CONVEX_RADIUS_DISPLAY_ROUNDED = 2
    }

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
        IS_TRIANGLE_OR_QUAD_SHAPE = 512,
        IS_QUAD_SHAPE = 1024,
        IS_SDF_EDGE_COLLISION_ENABLED = 2048,
        HAS_SURFACE_VELOCITY = 32768
    }

    public class MassConfig : hkReferencedObject
    {
        public hknpShape.MassConfig.Quality m_quality;

        public float m_inertiaFactor;

        public float m_massOrNegativeDensity;


        public enum Quality : int
        {
            QUALITY_LOW = 0,
            QUALITY_MEDIUM = 1,
            QUALITY_HIGH = 2
        }

    }


}

