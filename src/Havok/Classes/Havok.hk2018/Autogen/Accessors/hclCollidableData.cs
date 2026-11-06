// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclCollidableData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclCollidable>
{
    public hclCollidableData(Havok.hk2018.hclCollidable instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
                return true;
            }
            case "m_linearVelocity":
            case "linearVelocity":
            {
                value = instance.m_linearVelocity;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                value = instance.m_angularVelocity;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                value = instance.m_shape;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_pinchDetectionRadius":
            case "pinchDetectionRadius":
            {
                value = instance.m_pinchDetectionRadius;
                return true;
            }
            case "m_pinchDetectionPriority":
            case "pinchDetectionPriority":
            {
                value = instance.m_pinchDetectionPriority;
                return true;
            }
            case "m_pinchDetectionEnabled":
            case "pinchDetectionEnabled":
            {
                value = instance.m_pinchDetectionEnabled;
                return true;
            }
            case "m_virtualCollisionPointCollisionEnabled":
            case "virtualCollisionPointCollisionEnabled":
            {
                value = instance.m_virtualCollisionPointCollisionEnabled;
                return true;
            }
            case "m_enabled":
            case "enabled":
            {
                value = instance.m_enabled;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_linearVelocity":
            case "linearVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_linearVelocity = castValue;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_angularVelocity = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is Havok.hk2018.hclShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_pinchDetectionRadius":
            case "pinchDetectionRadius":
            {
                if (value is not float castValue) return false;
                instance.m_pinchDetectionRadius = castValue;
                return true;
            }
            case "m_pinchDetectionPriority":
            case "pinchDetectionPriority":
            {
                if (value is not sbyte castValue) return false;
                instance.m_pinchDetectionPriority = castValue;
                return true;
            }
            case "m_pinchDetectionEnabled":
            case "pinchDetectionEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_pinchDetectionEnabled = castValue;
                return true;
            }
            case "m_virtualCollisionPointCollisionEnabled":
            case "virtualCollisionPointCollisionEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_virtualCollisionPointCollisionEnabled = castValue;
                return true;
            }
            case "m_enabled":
            case "enabled":
            {
                if (value is not bool castValue) return false;
                instance.m_enabled = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
