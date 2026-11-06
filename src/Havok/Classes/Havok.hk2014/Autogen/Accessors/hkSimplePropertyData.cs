// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkSimplePropertyData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkSimpleProperty>
{
    public hkSimplePropertyData(Havok.hk2014.hkSimpleProperty instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_key":
            case "key":
            {
                value = instance.m_key;
                return true;
            }
            case "m_value":
            case "value":
            {
                value = instance.m_value;
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
            case "m_key":
            case "key":
            {
                if (value is not uint castValue) return false;
                instance.m_key = castValue;
                return true;
            }
            case "m_value":
            case "value":
            {
                if (value is not Havok.hk2014.hkSimplePropertyValue castValue) return false;
                instance.m_value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
