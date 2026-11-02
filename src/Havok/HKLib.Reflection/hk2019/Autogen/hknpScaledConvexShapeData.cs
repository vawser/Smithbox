// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpScaledConvexShapeData : HavokData<HKLib.hk2019.hknpScaledConvexShape> 
{
    public hknpScaledConvexShapeData(HavokType type, HKLib.hk2019.hknpScaledConvexShape instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_flags is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_type is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_numShapeKeyBits":
            case "numShapeKeyBits":
            {
                if (instance.m_numShapeKeyBits is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dispatchType":
            case "dispatchType":
            {
                if (instance.m_dispatchType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_dispatchType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_convexRadius":
            case "convexRadius":
            {
                if (instance.m_convexRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_properties":
            case "properties":
            {
                if (instance.m_properties is null)
                {
                    return true;
                }
                if (instance.m_properties is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_coreShape":
            case "coreShape":
            {
                if (instance.m_coreShape is null)
                {
                    return true;
                }
                if (instance.m_coreShape is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_scale":
            case "scale":
            {
                if (instance.m_scale is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_translation":
            case "translation":
            {
                if (instance.m_translation is not TGet castValue) return false;
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
            case "m_flags":
            case "flags":
            {
                if (value is HKLib.hk2019.hknpShape.FlagsEnum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (HKLib.hk2019.hknpShape.FlagsEnum)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_type":
            case "type":
            {
                if (value is HKLib.hk2019.hknpShapeType.Enum castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (HKLib.hk2019.hknpShapeType.Enum)byteValue;
                    return true;
                }
                return false;
            }
            case "m_numShapeKeyBits":
            case "numShapeKeyBits":
            {
                if (value is not byte castValue) return false;
                instance.m_numShapeKeyBits = castValue;
                return true;
            }
            case "m_dispatchType":
            case "dispatchType":
            {
                if (value is HKLib.hk2019.hknpCollisionDispatchType.Enum castValue)
                {
                    instance.m_dispatchType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_dispatchType = (HKLib.hk2019.hknpCollisionDispatchType.Enum)byteValue;
                    return true;
                }
                return false;
            }
            case "m_convexRadius":
            case "convexRadius":
            {
                if (value is not float castValue) return false;
                instance.m_convexRadius = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_properties":
            case "properties":
            {
                if (value is null)
                {
                    instance.m_properties = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkRefCountedProperties castValue)
                {
                    instance.m_properties = castValue;
                    return true;
                }
                return false;
            }
            case "m_coreShape":
            case "coreShape":
            {
                if (value is null)
                {
                    instance.m_coreShape = default;
                    return true;
                }
                if (value is HKLib.hk2019.hknpShape castValue)
                {
                    instance.m_coreShape = castValue;
                    return true;
                }
                return false;
            }
            case "m_scale":
            case "scale":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_scale = castValue;
                return true;
            }
            case "m_translation":
            case "translation":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_translation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
