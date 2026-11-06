// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkStringObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkStringObject>
{
    public hkStringObjectData(Havok.hk2014.hkStringObject instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
