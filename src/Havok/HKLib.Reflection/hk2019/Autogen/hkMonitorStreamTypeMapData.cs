// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkMonitorStreamTypeMapData : HavokData<HKLib.hk2019.hkMonitorStreamTypeMap> 
{
    public hkMonitorStreamTypeMapData(HavokType type, HKLib.hk2019.hkMonitorStreamTypeMap instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_map":
            case "map":
            {
                if (instance.m_map is not TGet castValue) return false;
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
            case "m_map":
            case "map":
            {
                if (value is not List<HKLib.hk2019.hkMonitorStreamTypeMap.TypeMap> castValue) return false;
                instance.m_map = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
