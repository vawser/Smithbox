// Automatically Generated

namespace HKLib.hk2018;

public class hknpBodyQuality : hkReferencedObject
{
    public int m_priority;

    public hknpBodyQuality.FlagsEnum m_supportedFlags;

    public hknpBodyQuality.FlagsEnum m_requestedFlags;

    public float m_contactCachingRelativeMovementThreshold;

    public hknpMotionRangeBreachPolicy.Enum m_motionRangeBreachPolicy;

    public hknpMotionRangeBreachPolicy.Enum m_motionWeldBreachPolicy;


    [Flags]
    public enum FlagsEnum : int
    {
        FORCE_GSK_EXECUTION = 2,
        FORCE_GSK_SINGLE_POINT_MANIFOLD = 4,
        ALLOW_INTERIOR_TRIANGLE_COLLISIONS = 8,
        USE_HIGHER_QUALITY_CONTACT_SOLVING = 64,
        ENABLE_NEIGHBOR_WELDING = 128,
        ENABLE_MOTION_WELDING = 256,
        ENABLE_TRIANGLE_WELDING = 512,
        ANY_PAIR_WELDING = 768,
        ANY_WELDING = 896,
        ENABLE_CONTACT_CACHING = 1024,
        REQUEST_LINEAR_ONLY_COLLISION_LOOK_AHEAD = 2048,
        FORCE_LINEAR_ONLY_COLLISION_LOOK_AHEAD = 4096,
        ENABLE_FRICTION_ESTIMATION = 8192,
        MERGE_FRICTION_JACOBIANS = 16384,
        DO_ONE_INACTIVE_COLLIDE_STEP = 32768,
        FIRST_NON_CACHABLE_FLAG = 65536,
        CLIP_ANGULAR_VELOCITY = 65536,
        USE_DISCRETE_AABB_EXPANSION = 131072,
        POST_SOLVE_LINEAR_CAST = 524288
    }

}

