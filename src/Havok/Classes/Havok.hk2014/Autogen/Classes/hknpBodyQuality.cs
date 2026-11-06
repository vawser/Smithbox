// Automatically Generated

namespace Havok.hk2014;

public class hknpBodyQuality : IHavokObject
{
    public int m_priority;

    public hknpBodyQuality.FlagsEnum m_supportedFlags;

    public hknpBodyQuality.FlagsEnum m_requestedFlags;

    public float m_contactCachingRelativeMovementThreshold;


    [Flags]
    public enum FlagsEnum : int
    {
        DROP_MANIFOLD_CACHE = 1,
        FORCE_GSK_EXECUTION = 2,
        FORCE_GSK_SINGLE_POINT_MANIFOLD = 4,
        ALLOW_INTERIOR_TRIANGLE_COLLISIONS = 8,
        ENABLE_SILHOUETTE_MANIFOLDS = 16,
        USE_HIGHER_QUALITY_CONTACT_SOLVING = 64,
        ENABLE_NEIGHBOR_WELDING = 128,
        ENABLE_MOTION_WELDING = 256,
        ENABLE_TRIANGLE_WELDING = 512,
        ANY_WELDING = 896,
        ENABLE_CONTACT_CACHING = 1024,
        REQUEST_LINEAR_ONLY_COLLISION_LOOK_AHEAD = 2048,
        FORCE_LINEAR_ONLY_COLLISION_LOOK_AHEAD = 4096,
        ENABLE_FRICTION_ESTIMATION = 8192,
        FIRST_NON_CACHABLE_FLAG = 65536,
        CLIP_ANGULAR_VELOCITY = 65536,
        USE_DISCRETE_AABB_EXPANSION = 131072,
        MERGE_FRICTION_JACOBIANS = 262144
    }

}
