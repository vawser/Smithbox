// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkMonitorStreamStringMapData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkMonitorStreamStringMap>
{
    public hkMonitorStreamStringMapData(Havok.hk2018.hkMonitorStreamStringMap instance) : base(instance) { }

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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_map":
            case "map":
            {
                if (value is not List<Havok.hk2018.hkMonitorStreamStringMap.StringMap> castValue) return false;
                instance.m_map = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
