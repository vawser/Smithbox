// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkTimerVolumeUtilsSphereData : HavokData<HKLib.hk2019.hkTimerVolumeUtils.Sphere> 
{
    public hkTimerVolumeUtilsSphereData(HavokType type, HKLib.hk2019.hkTimerVolumeUtils.Sphere instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_position":
            case "position":
            {
                if (instance.m_position is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (instance.m_radius is not TGet castValue) return false;
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
            case "m_position":
            case "position":
            {
                if (value is not HKLib.hk2019.hkFloat3 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
