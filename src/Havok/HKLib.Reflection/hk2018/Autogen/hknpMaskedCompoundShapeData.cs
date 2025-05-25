// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpMaskedCompoundShapeData : HavokData<hknpMaskedCompoundShape> 
{
    public hknpMaskedCompoundShapeData(HavokType type, hknpMaskedCompoundShape instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_mask":
            case "mask":
            {
                if (instance.m_mask is null)
                {
                    return true;
                }
                if (instance.m_mask is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_maskWrapper":
            case "maskWrapper":
            {
                if (instance.m_maskWrapper is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (instance.m_aabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isEnabledInstanceCacheEnabled":
            case "isEnabledInstanceCacheEnabled":
            {
                if (instance.m_isEnabledInstanceCacheEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enabledInstanceCache":
            case "enabledInstanceCache":
            {
                if (instance.m_enabledInstanceCache is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is hknpShape.FlagsEnum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (hknpShape.FlagsEnum)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_type":
            case "type":
            {
                if (value is hknpShapeType.Enum castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (hknpShapeType.Enum)byteValue;
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
                if (value is hknpCollisionDispatchType.Enum castValue)
                {
                    instance.m_dispatchType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_dispatchType = (hknpCollisionDispatchType.Enum)byteValue;
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
                if (value is hkRefCountedProperties castValue)
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
                if (value is hknpShape castValue)
                {
                    instance.m_coreShape = castValue;
                    return true;
                }
                return false;
            }
            case "m_mask":
            case "mask":
            {
                if (value is null)
                {
                    instance.m_mask = default;
                    return true;
                }
                if (value is hknpShapeKeyMask castValue)
                {
                    instance.m_mask = castValue;
                    return true;
                }
                return false;
            }
            case "m_maskWrapper":
            case "maskWrapper":
            {
                if (value is not hknpMaskedShape.MaskWrapper castValue) return false;
                instance.m_maskWrapper = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (value is not hkAabb castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            case "m_isEnabledInstanceCacheEnabled":
            case "isEnabledInstanceCacheEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_isEnabledInstanceCacheEnabled = castValue;
                return true;
            }
            case "m_enabledInstanceCache":
            case "enabledInstanceCache":
            {
                if (value is not List<hknpShapeInstanceId> castValue) return false;
                instance.m_enabledInstanceCache = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
