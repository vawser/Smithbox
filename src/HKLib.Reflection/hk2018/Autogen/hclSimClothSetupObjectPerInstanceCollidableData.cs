// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclSimClothSetupObjectPerInstanceCollidableData : HavokData<hclSimClothSetupObject.PerInstanceCollidable> 
{
    public hclSimClothSetupObjectPerInstanceCollidableData(HavokType type, hclSimClothSetupObject.PerInstanceCollidable instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_collidable":
            case "collidable":
            {
                if (instance.m_collidable is null)
                {
                    return true;
                }
                if (instance.m_collidable is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_collidingParticles":
            case "collidingParticles":
            {
                if (instance.m_collidingParticles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_drivingBoneName":
            case "drivingBoneName":
            {
                if (instance.m_drivingBoneName is null)
                {
                    return true;
                }
                if (instance.m_drivingBoneName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
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
            case "m_vcpCollisionEnabled":
            case "vcpCollisionEnabled":
            {
                if (instance.m_vcpCollisionEnabled is not TGet castValue) return false;
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
            case "m_collidable":
            case "collidable":
            {
                if (value is null)
                {
                    instance.m_collidable = default;
                    return true;
                }
                if (value is hclCollidable castValue)
                {
                    instance.m_collidable = castValue;
                    return true;
                }
                return false;
            }
            case "m_collidingParticles":
            case "collidingParticles":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_collidingParticles = castValue;
                return true;
            }
            case "m_drivingBoneName":
            case "drivingBoneName":
            {
                if (value is null)
                {
                    instance.m_drivingBoneName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_drivingBoneName = castValue;
                    return true;
                }
                return false;
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
            case "m_vcpCollisionEnabled":
            case "vcpCollisionEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_vcpCollisionEnabled = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
