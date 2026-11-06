// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpCompositeShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpCompositeShape>
{
    public hknpCompositeShapeData(Havok.hk2019.hknpCompositeShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_numShapeKeyBits":
            case "numShapeKeyBits":
            {
                value = instance.m_numShapeKeyBits;
                return true;
            }
            case "m_dispatchType":
            case "dispatchType":
            {
                value = instance.m_dispatchType;
                return true;
            }
            case "m_convexRadius":
            case "convexRadius":
            {
                value = instance.m_convexRadius;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_properties":
            case "properties":
            {
                value = instance.m_properties;
                return true;
            }
            case "m_shapeTagCodecInfo":
            case "shapeTagCodecInfo":
            {
                value = instance.m_shapeTagCodecInfo;
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
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2019.hknpShape.FlagsEnum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (Havok.hk2019.hknpShape.FlagsEnum)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2019.hknpShapeType.Enum castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2019.hknpShapeType.Enum)byteValue;
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
                if (value is Havok.hk2019.hknpCollisionDispatchType.Enum castValue)
                {
                    instance.m_dispatchType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_dispatchType = (Havok.hk2019.hknpCollisionDispatchType.Enum)byteValue;
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
                if (value is Havok.hk2019.hkRefCountedProperties castValue)
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
            default:
            return false;
        }
    }

}
