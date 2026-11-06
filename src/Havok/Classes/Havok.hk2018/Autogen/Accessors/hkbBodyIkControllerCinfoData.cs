// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbBodyIkControllerCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbBodyIkControllerCinfo>
{
    public hkbBodyIkControllerCinfoData(Havok.hk2018.hkbBodyIkControllerCinfo instance) : base(instance) { }

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
            case "m_profiles":
            case "profiles":
            {
                value = instance.m_profiles;
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
            case "m_profiles":
            case "profiles":
            {
                if (value is not List<Havok.hk2018.hkbBodyIkControllerProfile> castValue) return false;
                instance.m_profiles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
