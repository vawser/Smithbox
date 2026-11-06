// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkMonitorStreamStringMapStringMapData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkMonitorStreamStringMap.StringMap>
{
    public hkMonitorStreamStringMapStringMapData(Havok.hk2019.hkMonitorStreamStringMap.StringMap instance) : base(instance) { }

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
            case "m_string":
            case "string":
            {
                value = instance.m_string;
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
            case "m_string":
            case "string":
            {
                if (value is null)
                {
                    instance.m_string = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_string = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
