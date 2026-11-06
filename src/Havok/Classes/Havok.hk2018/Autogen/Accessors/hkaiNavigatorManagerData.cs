// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavigatorManagerData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavigatorManager>
{
    public hkaiNavigatorManagerData(Havok.hk2018.hkaiNavigatorManager instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_world":
            case "world":
            {
                value = instance.m_world;
                return true;
            }
            case "m_navigators":
            case "navigators":
            {
                value = instance.m_navigators;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_world":
            case "world":
            {
                if (value is null)
                {
                    instance.m_world = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiWorld castValue)
                {
                    instance.m_world = castValue;
                    return true;
                }
                return false;
            }
            case "m_navigators":
            case "navigators":
            {
                if (value is not List<Havok.hk2018.hkaiNavigator?> castValue) return false;
                instance.m_navigators = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
