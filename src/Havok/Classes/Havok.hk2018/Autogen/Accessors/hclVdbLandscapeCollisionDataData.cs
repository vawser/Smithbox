// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclVdbLandscapeCollisionDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclVdbLandscapeCollisionData>
{
    public hclVdbLandscapeCollisionDataData(Havok.hk2018.hclVdbLandscapeCollisionData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_landscapeRadius":
            case "landscapeRadius":
            {
                value = instance.m_landscapeRadius;
                return true;
            }
            case "m_enableStuckParticleDetection":
            case "enableStuckParticleDetection":
            {
                value = instance.m_enableStuckParticleDetection;
                return true;
            }
            case "m_stuckParticlesStretchFactorSq":
            case "stuckParticlesStretchFactorSq":
            {
                value = instance.m_stuckParticlesStretchFactorSq;
                return true;
            }
            case "m_pinchDetectionEnabled":
            case "pinchDetectionEnabled":
            {
                value = instance.m_pinchDetectionEnabled;
                return true;
            }
            case "m_pinchDetectionPriority":
            case "pinchDetectionPriority":
            {
                value = instance.m_pinchDetectionPriority;
                return true;
            }
            case "m_pinchDetectionRadius":
            case "pinchDetectionRadius":
            {
                value = instance.m_pinchDetectionRadius;
                return true;
            }
            case "m_collisionTolerance":
            case "collisionTolerance":
            {
                value = instance.m_collisionTolerance;
                return true;
            }
            case "m_overridden":
            case "overridden":
            {
                value = instance.m_overridden;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_landscapeRadius":
            case "landscapeRadius":
            {
                if (value is not float castValue) return false;
                instance.m_landscapeRadius = castValue;
                return true;
            }
            case "m_enableStuckParticleDetection":
            case "enableStuckParticleDetection":
            {
                if (value is not bool castValue) return false;
                instance.m_enableStuckParticleDetection = castValue;
                return true;
            }
            case "m_stuckParticlesStretchFactorSq":
            case "stuckParticlesStretchFactorSq":
            {
                if (value is not float castValue) return false;
                instance.m_stuckParticlesStretchFactorSq = castValue;
                return true;
            }
            case "m_pinchDetectionEnabled":
            case "pinchDetectionEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_pinchDetectionEnabled = castValue;
                return true;
            }
            case "m_pinchDetectionPriority":
            case "pinchDetectionPriority":
            {
                if (value is not sbyte castValue) return false;
                instance.m_pinchDetectionPriority = castValue;
                return true;
            }
            case "m_pinchDetectionRadius":
            case "pinchDetectionRadius":
            {
                if (value is not float castValue) return false;
                instance.m_pinchDetectionRadius = castValue;
                return true;
            }
            case "m_collisionTolerance":
            case "collisionTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_collisionTolerance = castValue;
                return true;
            }
            case "m_overridden":
            case "overridden":
            {
                if (value is not bool castValue) return false;
                instance.m_overridden = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
