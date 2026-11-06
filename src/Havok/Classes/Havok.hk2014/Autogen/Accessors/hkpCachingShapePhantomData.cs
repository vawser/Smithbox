// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpCachingShapePhantomData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpCachingShapePhantom>
{
    public hkpCachingShapePhantomData(Havok.hk2014.hkpCachingShapePhantom instance) : base(instance) { }

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
            case "m_motionState":
            case "motionState":
            {
                value = instance.m_motionState;
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
                if (value is not Havok.hk2014.hkpLinkedCollidable castValue) return false;
                instance.m_collidable = castValue;
                return true;
            }
            case "m_multiThreadCheck":
            case "multiThreadCheck":
            {
                if (value is not Havok.hk2014.hkMultiThreadCheck castValue) return false;
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
                if (value is not List<Havok.hk2014.hkSimpleProperty> castValue) return false;
                instance.m_properties = castValue;
                return true;
            }
            case "m_motionState":
            case "motionState":
            {
                if (value is not Havok.hk2014.hkMotionState castValue) return false;
                instance.m_motionState = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
