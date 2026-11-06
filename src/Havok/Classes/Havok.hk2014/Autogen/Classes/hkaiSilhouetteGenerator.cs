// Automatically Generated

namespace Havok.hk2014;

public class hkaiSilhouetteGenerator : hkReferencedObject
{
    public ulong m_userData;

    public float m_lazyRecomputeDisplacementThreshold;

    public hkaiSilhouetteGenerator.GeneratorType m_type;

    public byte m_forceGenerateOntoPpu;

    public int m_materialId;

    public hkaiConvexSilhouetteSet? m_cachedSilhouettes;

    public hkQTransform m_transform = new();


    public enum GeneratorType : int
    {
        GENERATOR_PHYSICS_2012_BODY = 0,
        GENERATOR_POINT_CLOUD = 1,
        GENERATOR_STORAGE_POINT_CLOUD_REMOVED = 2,
        GENERATOR_PHYSICS_BODY_BASE = 3,
        GENERATOR_PHYSICS_BODY = 4,
        GENERATOR_USER_0 = 5,
        GENERATOR_USER_1 = 6,
        GENERATOR_USER_2 = 7,
        GENERATOR_USER_3 = 8,
        GENERATOR_MAX = 9
    }

}
