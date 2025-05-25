// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCharacterControlCommandData : HavokData<hkbCharacterControlCommand> 
{
    public hkbCharacterControlCommandData(HavokType type, hkbCharacterControlCommand instance) : base(type, instance) {}

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
            case "m_command":
            case "command":
            {
                if (instance.m_command is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_command is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_padding":
            case "padding":
            {
                if (instance.m_padding is not TGet castValue) return false;
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
            case "m_command":
            case "command":
            {
                if (value is hkbCharacterControlCommand.CharacterControlCommand castValue)
                {
                    instance.m_command = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_command = (hkbCharacterControlCommand.CharacterControlCommand)byteValue;
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
