// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkaBoneData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkaBone>
{
    public hkaBoneData(Havok.hk2016.hkaBone instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_lockTranslation":
            case "lockTranslation":
            {
                value = instance.m_lockTranslation;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_lockTranslation":
            case "lockTranslation":
            {
                if (value is not bool castValue) return false;
                instance.m_lockTranslation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
