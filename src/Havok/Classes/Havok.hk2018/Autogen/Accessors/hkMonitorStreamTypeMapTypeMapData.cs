// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkMonitorStreamTypeMapTypeMapData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkMonitorStreamTypeMap.TypeMap>
{
    public hkMonitorStreamTypeMapTypeMapData(Havok.hk2018.hkMonitorStreamTypeMap.TypeMap instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                value = instance.m_id;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
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
            case "m_id":
            case "id":
            {
                if (value is not ulong castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is null)
                {
                    instance.m_type = default;
                    return true;
                }
                if (value is Havok.hk2018.IHavokObject castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
