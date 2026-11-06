// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbRaiseEventCommandData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbRaiseEventCommand>
{
    public hkbRaiseEventCommandData(Havok.hk2018.hkbRaiseEventCommand instance) : base(instance) { }

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
            case "m_characterId":
            case "characterId":
            {
                value = instance.m_characterId;
                return true;
            }
            case "m_global":
            case "global":
            {
                value = instance.m_global;
                return true;
            }
            case "m_externalId":
            case "externalId":
            {
                value = instance.m_externalId;
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
