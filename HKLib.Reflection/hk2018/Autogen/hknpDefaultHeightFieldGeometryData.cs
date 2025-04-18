// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpDefaultHeightFieldGeometryData : HavokData<hknpDefaultHeightFieldGeometry> 
{
    public hknpDefaultHeightFieldGeometryData(HavokType type, hknpDefaultHeightFieldGeometry instance) : base(type, instance) {}

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
            case "m_storage":
            case "storage":
            {
                if (instance.m_storage is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shapeTags":
            case "shapeTags":
            {
                if (instance.m_shapeTags is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fold":
            case "fold":
            {
                if (instance.m_fold is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_fold is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_resX":
            case "resX":
            {
                if (instance.m_resX is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_resZ":
            case "resZ":
            {
                if (instance.m_resZ is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_heightScale":
            case "heightScale":
            {
                if (instance.m_heightScale is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_heightOffset":
            case "heightOffset":
            {
                if (instance.m_heightOffset is not TGet castValue) return false;
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
                if (value is hknpHeightFieldGeometry.Fold castValue)
                {
                    instance.m_fold = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_fold = (hknpHeightFieldGeometry.Fold)intValue;
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
