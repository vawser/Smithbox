// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiInvertedAabbVolumeData : HavokData<HKLib.hk2019.hkaiInvertedAabbVolume> 
{
    public hkaiInvertedAabbVolumeData(HavokType type, HKLib.hk2019.hkaiInvertedAabbVolume instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aabb":
            case "aabb":
            {
                if (instance.m_aabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_geometry":
            case "geometry":
            {
                if (instance.m_geometry is not TGet castValue) return false;
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
            case "m_aabb":
            case "aabb":
            {
                if (value is not HKLib.hk2019.hkAabb castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            case "m_geometry":
            case "geometry":
            {
                if (value is not HKLib.hk2019.hkGeometry castValue) return false;
                instance.m_geometry = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
