// Automatically Generated

namespace Havok.hk2014;

public class hknpMaterial : IHavokObject
{
    public string? m_name;

    public uint m_isExclusive;

    public int m_flags;

    public hknpMaterial.TriggerType m_triggerType;

    public hkUFloat8 m_triggerManifoldTolerance = new();

    public float m_dynamicFriction;

    public float m_staticFriction;

    public float m_restitution;

    public hknpMaterial.CombinePolicy m_frictionCombinePolicy;

    public hknpMaterial.CombinePolicy m_restitutionCombinePolicy;

    public float m_weldingTolerance;

    public float m_maxContactImpulse;

    public float m_fractionOfClippedImpulseToApply;

    public hknpMaterial.MassChangerCategory m_massChangerCategory;

    public float m_massChangerHeavyObjectFactor;

    public float m_softContactForceFactor;

    public float m_softContactDampFactor;

    public hkUFloat8 m_softContactSeperationVelocity = new();

    public hknpSurfaceVelocity? m_surfaceVelocity;

    public float m_disablingCollisionsBetweenCvxCvxDynamicObjectsDistance;

    public ulong m_userData;

    public bool m_isShared;


    public enum TriggerType : int
    {
        TRIGGER_TYPE_NONE = 0,
        TRIGGER_TYPE_BROAD_PHASE = 1,
        TRIGGER_TYPE_NARROW_PHASE = 2,
        TRIGGER_TYPE_CONTACT_SOLVER = 3
    }

    public enum CombinePolicy : int
    {
        COMBINE_AVG = 0,
        COMBINE_MIN = 1,
        COMBINE_MAX = 2
    }

    public enum MassChangerCategory : int
    {
        MASS_CHANGER_IGNORE = 0,
        MASS_CHANGER_DEBRIS = 1,
        MASS_CHANGER_HEAVY = 2
    }

}
