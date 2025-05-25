// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpCompoundShapeData : HavokData<hknpCompoundShape> 
{
    public hknpCompoundShapeData(HavokType type, hknpCompoundShape instance) : base(type, instance) {}

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
            case "m_shapeTagCodecInfo":
            case "shapeTagCodecInfo":
            {
                if (instance.m_shapeTagCodecInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_materialTable":
            case "materialTable":
            {
                if (instance.m_materialTable is null)
                {
                    return true;
                }
                if (instance.m_materialTable is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_instances":
            case "instances":
            {
                if (instance.m_instances is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instanceVelocities":
            case "instanceVelocities":
            {
                if (instance.m_instanceVelocities is not TGet castValue) return false;
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
            case "m_boundingRadius":
            case "boundingRadius":
            {
                if (instance.m_boundingRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isMutable":
            case "isMutable":
            {
                if (instance.m_isMutable is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_estimatedNumShapeKeys":
            case "estimatedNumShapeKeys":
            {
                if (instance.m_estimatedNumShapeKeys is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boundingVolumeData":
            case "boundingVolumeData":
            {
                if (instance.m_boundingVolumeData is not TGet castValue) return false;
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
            case "m_shapeTagCodecInfo":
            case "shapeTagCodecInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_shapeTagCodecInfo = castValue;
                return true;
            }
            case "m_materialTable":
            case "materialTable":
            {
                if (value is null)
                {
                    instance.m_materialTable = default;
                    return true;
                }
                if (value is hkReferencedObject castValue)
                {
                    instance.m_materialTable = castValue;
                    return true;
                }
                return false;
            }
            case "m_instances":
            case "instances":
            {
                if (value is not hkFreeListArrayHknpShapeInstance castValue) return false;
                instance.m_instances = castValue;
                return true;
            }
            case "m_instanceVelocities":
            case "instanceVelocities":
            {
                if (value is not List<hknpCompoundShape.VelocityInfo> castValue) return false;
                instance.m_instanceVelocities = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (value is not hkAabb castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            case "m_boundingRadius":
            case "boundingRadius":
            {
                if (value is not float castValue) return false;
                instance.m_boundingRadius = castValue;
                return true;
            }
            case "m_isMutable":
            case "isMutable":
            {
                if (value is not bool castValue) return false;
                instance.m_isMutable = castValue;
                return true;
            }
            case "m_estimatedNumShapeKeys":
            case "estimatedNumShapeKeys":
            {
                if (value is not int castValue) return false;
                instance.m_estimatedNumShapeKeys = castValue;
                return true;
            }
            case "m_boundingVolumeData":
            case "boundingVolumeData":
            {
                if (value is not HKLib.hk2018.hknpCompoundShapeData castValue) return false;
                instance.m_boundingVolumeData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
