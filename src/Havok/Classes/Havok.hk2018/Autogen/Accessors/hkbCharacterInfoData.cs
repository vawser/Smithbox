// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbCharacterInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbCharacterInfo>
{
    public hkbCharacterInfoData(Havok.hk2018.hkbCharacterInfo instance) : base(instance) { }

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
            case "m_event":
            case "event":
            {
                value = instance.m_event;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                value = instance.m_padding;
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
            case "m_event":
            case "event":
            {
                if (value is Havok.hk2018.hkbCharacterInfo.Event castValue)
                {
                    instance.m_event = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_event = (Havok.hk2018.hkbCharacterInfo.Event)byteValue;
                    return true;
                }
                return false;
            }
            case "m_padding":
            case "padding":
            {
                if (value is not int castValue) return false;
                instance.m_padding = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
