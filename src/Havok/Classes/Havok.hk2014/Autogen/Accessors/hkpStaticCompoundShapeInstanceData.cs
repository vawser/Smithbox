// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpStaticCompoundShapeInstanceData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpStaticCompoundShapeInstance>
{
    public hkpStaticCompoundShapeInstanceData(Havok.hk2014.hkpStaticCompoundShapeInstance instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                value = instance.m_shape;
                return true;
            }
            case "m_filterInfo":
            case "filterInfo":
            {
                value = instance.m_filterInfo;
                return true;
            }
            case "m_childFilterInfoMask":
            case "childFilterInfoMask":
            {
                value = instance.m_childFilterInfoMask;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
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
            case "m_transform":
            case "transform":
            {
                if (value is not Havok.hk2014.hkQsTransform castValue) return false;
                instance.m_transform = castValue;
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
                if (value is Havok.hk2014.hkpShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_filterInfo":
            case "filterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_filterInfo = castValue;
                return true;
            }
            case "m_childFilterInfoMask":
            case "childFilterInfoMask":
            {
                if (value is not uint castValue) return false;
                instance.m_childFilterInfoMask = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
