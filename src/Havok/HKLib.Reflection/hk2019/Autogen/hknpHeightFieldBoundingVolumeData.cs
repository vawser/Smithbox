// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpHeightFieldBoundingVolumeData : HavokData<HKLib.hk2019.hknpHeightFieldBoundingVolume> 
{
    public hknpHeightFieldBoundingVolumeData(HavokType type, HKLib.hk2019.hknpHeightFieldBoundingVolume instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_minMaxTree":
            case "minMaxTree":
            {
                if (value is not HKLib.hk2019.hknpMinMaxQuadTree castValue) return false;
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
