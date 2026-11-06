// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpScaledConvexShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpScaledConvexShape>
{
    public hknpScaledConvexShapeData(Havok.hk2014.hknpScaledConvexShape instance) : base(instance) { }

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
            case "m_coreShape":
            case "coreShape":
            {
                value = instance.m_coreShape;
                return true;
            }
            case "m_coreShapeSize":
            case "coreShapeSize":
            {
                value = instance.m_coreShapeSize;
                return true;
            }
            case "m_scale":
            case "scale":
            {
                value = instance.m_scale;
                return true;
            }
            case "m_translation":
            case "translation":
            {
                value = instance.m_translation;
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
                if (value is Havok.hk2014.hknpShape.FlagsEnum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (Havok.hk2014.hknpShape.FlagsEnum)ushortValue;
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
                if (value is not byte castValue) return false;
                instance.m_dispatchType = castValue;
                return true;
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
                if (value is Havok.hk2014.hkRefCountedProperties castValue)
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
                if (value is Havok.hk2014.hknpShape castValue)
                {
                    instance.m_coreShape = castValue;
                    return true;
                }
                return false;
            }
            case "m_coreShapeSize":
            case "coreShapeSize":
            {
                if (value is not int castValue) return false;
                instance.m_coreShapeSize = castValue;
                return true;
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
