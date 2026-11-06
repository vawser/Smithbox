// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkRefCountedPropertiesEntryData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkRefCountedProperties.Entry>
{
    public hkRefCountedPropertiesEntryData(Havok.hk2018.hkRefCountedProperties.Entry instance) : base(instance) { }

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
            case "m_key":
            case "key":
            {
                value = instance.m_key;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
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
                if (value is Havok.hk2018.hkReferencedObject castValue)
                {
                    instance.m_object = castValue;
                    return true;
                }
                return false;
            }
            case "m_key":
            case "key":
            {
                if (value is not ushort castValue) return false;
                instance.m_key = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not ushort castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
