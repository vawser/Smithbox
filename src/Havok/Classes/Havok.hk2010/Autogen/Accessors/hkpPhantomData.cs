// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkpPhantomData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkpPhantom>
{
    public hkpPhantomData(Havok.hk2010.hkpPhantom instance) : base(instance) { }

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
            case "m_collidable":
            case "collidable":
            {
                value = instance.m_collidable;
                return true;
            }
            case "m_multiThreadCheck":
            case "multiThreadCheck":
            {
                value = instance.m_multiThreadCheck;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_properties":
            case "properties":
            {
                value = instance.m_properties;
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
            case "m_collidable":
            case "collidable":
            {
                if (value is not Havok.hk2010.hkpLinkedCollidable castValue) return false;
                instance.m_collidable = castValue;
                return true;
            }
            case "m_multiThreadCheck":
            case "multiThreadCheck":
            {
                if (value is not Havok.hk2010.hkMultiThreadCheck castValue) return false;
                instance.m_multiThreadCheck = castValue;
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
            case "m_properties":
            case "properties":
            {
                if (value is not List<Havok.hk2010.hkpProperty> castValue) return false;
                instance.m_properties = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
