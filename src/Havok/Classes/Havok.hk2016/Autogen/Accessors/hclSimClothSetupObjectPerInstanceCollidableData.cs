// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclSimClothSetupObjectPerInstanceCollidableData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclSimClothSetupObject.PerInstanceCollidable>
{
    public hclSimClothSetupObjectPerInstanceCollidableData(Havok.hk2016.hclSimClothSetupObject.PerInstanceCollidable instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_collidable":
            case "collidable":
            {
                value = instance.m_collidable;
                return true;
            }
            case "m_collidingParticles":
            case "collidingParticles":
            {
                value = instance.m_collidingParticles;
                return true;
            }
            case "m_drivingBoneName":
            case "drivingBoneName":
            {
                value = instance.m_drivingBoneName;
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
            case "m_vcpCollisionEnabled":
            case "vcpCollisionEnabled":
            {
                value = instance.m_vcpCollisionEnabled;
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
            case "m_collidable":
            case "collidable":
            {
                if (value is null)
                {
                    instance.m_collidable = default;
                    return true;
                }
                if (value is Havok.hk2016.hclCollidable castValue)
                {
                    instance.m_collidable = castValue;
                    return true;
                }
                return false;
            }
            case "m_collidingParticles":
            case "collidingParticles":
            {
                if (value is not Havok.hk2016.hclVertexSelectionInput castValue) return false;
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
