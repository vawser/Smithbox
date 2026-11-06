// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkVariantData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkVariant>
{
    public hkVariantData(Havok.hk2014.hkVariant instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_object":
            case "object":
            {
                value = instance.m_object;
                return true;
            }
            case "m_class":
            case "class":
            {
                value = instance.m_class;
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
            case "m_object":
            case "object":
            {
                if (value is null)
                {
                    instance.m_object = default;
                    return true;
                }
                if (value is Havok.hk2014.hkReferencedObject castValue)
                {
                    instance.m_object = castValue;
                    return true;
                }
                return false;
            }
            case "m_class":
            case "class":
            {
                if (value is null)
                {
                    instance.m_class = default;
                    return true;
                }
                if (value is Havok.hk2014.hkClass castValue)
                {
                    instance.m_class = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
