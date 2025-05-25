// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVdbLandscapeCollisionDataData : HavokData<hclVdbLandscapeCollisionData> 
{
    public hclVdbLandscapeCollisionDataData(HavokType type, hclVdbLandscapeCollisionData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_landscapeRadius":
            case "landscapeRadius":
            {
                if (instance.m_landscapeRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableStuckParticleDetection":
            case "enableStuckParticleDetection":
            {
                if (instance.m_enableStuckParticleDetection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stuckParticlesStretchFactorSq":
            case "stuckParticlesStretchFactorSq":
            {
                if (instance.m_stuckParticlesStretchFactorSq is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pinchDetectionEnabled":
            case "pinchDetectionEnabled":
            {
                if (instance.m_pinchDetectionEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pinchDetectionPriority":
            case "pinchDetectionPriority":
            {
                if (instance.m_pinchDetectionPriority is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pinchDetectionRadius":
            case "pinchDetectionRadius":
            {
                if (instance.m_pinchDetectionRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collisionTolerance":
            case "collisionTolerance":
            {
                if (instance.m_collisionTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_overridden":
            case "overridden":
            {
                if (instance.m_overridden is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
