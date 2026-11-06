// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpDefaultHeightFieldGeometryData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpDefaultHeightFieldGeometry>
{
    public hknpDefaultHeightFieldGeometryData(Havok.hk2018.hknpDefaultHeightFieldGeometry instance) : base(instance) { }

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
            case "m_storage":
            case "storage":
            {
                value = instance.m_storage;
                return true;
            }
            case "m_shapeTags":
            case "shapeTags":
            {
                value = instance.m_shapeTags;
                return true;
            }
            case "m_fold":
            case "fold":
            {
                value = instance.m_fold;
                return true;
            }
            case "m_resX":
            case "resX":
            {
                value = instance.m_resX;
                return true;
            }
            case "m_resZ":
            case "resZ":
            {
                value = instance.m_resZ;
                return true;
            }
            case "m_heightScale":
            case "heightScale":
            {
                value = instance.m_heightScale;
                return true;
            }
            case "m_heightOffset":
            case "heightOffset":
            {
                value = instance.m_heightOffset;
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
            case "m_storage":
            case "storage":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_storage = castValue;
                return true;
            }
            case "m_shapeTags":
            case "shapeTags":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_shapeTags = castValue;
                return true;
            }
            case "m_fold":
            case "fold":
            {
                if (value is Havok.hk2018.hknpHeightFieldGeometry.Fold castValue)
                {
                    instance.m_fold = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_fold = (Havok.hk2018.hknpHeightFieldGeometry.Fold)intValue;
                    return true;
                }
                return false;
            }
            case "m_resX":
            case "resX":
            {
                if (value is not int castValue) return false;
                instance.m_resX = castValue;
                return true;
            }
            case "m_resZ":
            case "resZ":
            {
                if (value is not int castValue) return false;
                instance.m_resZ = castValue;
                return true;
            }
            case "m_heightScale":
            case "heightScale":
            {
                if (value is not float castValue) return false;
                instance.m_heightScale = castValue;
                return true;
            }
            case "m_heightOffset":
            case "heightOffset":
            {
                if (value is not float castValue) return false;
                instance.m_heightOffset = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
