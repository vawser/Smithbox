// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkMonitorStreamTypeMapData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkMonitorStreamTypeMap>
{
    public hkMonitorStreamTypeMapData(Havok.hk2019.hkMonitorStreamTypeMap instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_map":
            case "map":
            {
                value = instance.m_map;
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
            case "m_map":
            case "map":
            {
                if (value is not List<Havok.hk2019.hkMonitorStreamTypeMap.TypeMap> castValue) return false;
                instance.m_map = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
