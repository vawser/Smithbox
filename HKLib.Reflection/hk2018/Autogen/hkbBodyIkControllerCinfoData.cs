// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbBodyIkControllerCinfoData : HavokData<hkbBodyIkControllerCinfo> 
{
    public hkbBodyIkControllerCinfoData(HavokType type, hkbBodyIkControllerCinfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_profiles":
            case "profiles":
            {
                if (instance.m_profiles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_profiles":
            case "profiles":
            {
                if (value is not List<hkbBodyIkControllerProfile> castValue) return false;
                instance.m_profiles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
