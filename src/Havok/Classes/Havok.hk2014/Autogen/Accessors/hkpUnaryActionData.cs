// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpUnaryActionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpUnaryAction>
{
    public hkpUnaryActionData(Havok.hk2014.hkpUnaryAction instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_entity":
            case "entity":
            {
                value = instance.m_entity;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
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
            case "m_entity":
            case "entity":
            {
                if (value is null)
                {
                    instance.m_entity = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpEntity castValue)
                {
                    instance.m_entity = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
