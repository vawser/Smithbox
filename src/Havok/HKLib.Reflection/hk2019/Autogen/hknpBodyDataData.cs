// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpBodyDataData : HavokData<HKLib.hk2019.hknpBodyData> 
{
    public hknpBodyDataData(HavokType type, HKLib.hk2019.hknpBodyData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_shape":
            case "shape":
            {
                if (instance.m_shape is null)
                {
                    return true;
                }
                if (instance.m_shape is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_bodyQuality":
            case "bodyQuality":
            {
                if (instance.m_bodyQuality is null)
                {
                    return true;
                }
                if (instance.m_bodyQuality is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_motionPropertiesData":
            case "motionPropertiesData":
            {
                if (instance.m_motionPropertiesData is null)
                {
                    return true;
                }
                if (instance.m_motionPropertiesData is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_material":
            case "material":
            {
                if (instance.m_material is null)
                {
                    return true;
                }
                if (instance.m_material is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_initialPosition":
            case "initialPosition":
            {
                if (instance.m_initialPosition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_initialOrientation":
            case "initialOrientation":
            {
                if (instance.m_initialOrientation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_massProperties":
            case "massProperties":
            {
                if (instance.m_massProperties is not TGet castValue) return false;
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
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is HKLib.hk2019.hknpShape castValue)
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
                if (value is HKLib.hk2019.hknpBodyQuality castValue)
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
                if (value is HKLib.hk2019.hknpMotionPropertiesData castValue)
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
                if (value is HKLib.hk2019.hknpMaterial castValue)
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
                if (value is not HKLib.hk2019.hkMassProperties castValue) return false;
                instance.m_massProperties = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
