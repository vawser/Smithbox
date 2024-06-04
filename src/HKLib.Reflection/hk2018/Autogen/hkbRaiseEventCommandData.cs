// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbRaiseEventCommandData : HavokData<hkbRaiseEventCommand> 
{
    public hkbRaiseEventCommandData(HavokType type, hkbRaiseEventCommand instance) : base(type, instance) {}

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
            case "m_characterId":
            case "characterId":
            {
                if (instance.m_characterId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_global":
            case "global":
            {
                if (instance.m_global is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_externalId":
            case "externalId":
            {
                if (instance.m_externalId is not TGet castValue) return false;
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
            case "m_characterId":
            case "characterId":
            {
                if (value is not ulong castValue) return false;
                instance.m_characterId = castValue;
                return true;
            }
            case "m_global":
            case "global":
            {
                if (value is not bool castValue) return false;
                instance.m_global = castValue;
                return true;
            }
            case "m_externalId":
            case "externalId":
            {
                if (value is not int castValue) return false;
                instance.m_externalId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
