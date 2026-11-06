// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpBodyDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpBodyData>
{
    public hknpBodyDataData(Havok.hk2018.hknpBodyData instance) : base(instance) { }

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
            case "m_shape":
            case "shape":
            {
                value = instance.m_shape;
                return true;
            }
            case "m_bodyQuality":
            case "bodyQuality":
            {
                value = instance.m_bodyQuality;
                return true;
            }
            case "m_motionPropertiesData":
            case "motionPropertiesData":
            {
                value = instance.m_motionPropertiesData;
                return true;
            }
            case "m_material":
            case "material":
            {
                value = instance.m_material;
                return true;
            }
            case "m_initialPosition":
            case "initialPosition":
            {
                value = instance.m_initialPosition;
                return true;
            }
            case "m_initialOrientation":
            case "initialOrientation":
            {
                value = instance.m_initialOrientation;
                return true;
            }
            case "m_massProperties":
            case "massProperties":
            {
                value = instance.m_massProperties;
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
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is Havok.hk2018.hknpShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_bodyQuality":
            case "bodyQuality":
            {
                if (value is null)
                {
                    instance.m_bodyQuality = default;
                    return true;
                }
                if (value is Havok.hk2018.hknpBodyQuality castValue)
                {
                    instance.m_bodyQuality = castValue;
                    return true;
                }
                return false;
            }
            case "m_motionPropertiesData":
            case "motionPropertiesData":
            {
                if (value is null)
                {
                    instance.m_motionPropertiesData = default;
                    return true;
                }
                if (value is Havok.hk2018.hknpMotionPropertiesData castValue)
                {
                    instance.m_motionPropertiesData = castValue;
                    return true;
                }
                return false;
            }
            case "m_material":
            case "material":
            {
                if (value is null)
                {
                    instance.m_material = default;
                    return true;
                }
                if (value is Havok.hk2018.hknpMaterial castValue)
                {
                    instance.m_material = castValue;
                    return true;
                }
                return false;
            }
            case "m_initialPosition":
            case "initialPosition":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_initialPosition = castValue;
                return true;
            }
            case "m_initialOrientation":
            case "initialOrientation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_initialOrientation = castValue;
                return true;
            }
            case "m_massProperties":
            case "massProperties":
            {
                if (value is not Havok.hk2018.hkMassProperties castValue) return false;
                instance.m_massProperties = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
