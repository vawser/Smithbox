// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpHeightFieldBoundingVolumeData : HavokData<hknpHeightFieldBoundingVolume> 
{
    public hknpHeightFieldBoundingVolumeData(HavokType type, hknpHeightFieldBoundingVolume instance) : base(type, instance) {}

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
            case "m_minMaxTree":
            case "minMaxTree":
            {
                if (instance.m_minMaxTree is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minLevel":
            case "minLevel":
            {
                if (instance.m_minLevel is not TGet castValue) return false;
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
            case "m_minMaxTree":
            case "minMaxTree":
            {
                if (value is not hknpMinMaxQuadTree castValue) return false;
                instance.m_minMaxTree = castValue;
                return true;
            }
            case "m_minLevel":
            case "minLevel":
            {
                if (value is not int castValue) return false;
                instance.m_minLevel = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
