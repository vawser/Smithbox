// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbCharacterControlCommandData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbCharacterControlCommand>
{
    public hkbCharacterControlCommandData(Havok.hk2014.hkbCharacterControlCommand instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_characterId":
            case "characterId":
            {
                value = instance.m_characterId;
                return true;
            }
            case "m_command":
            case "command":
            {
                value = instance.m_command;
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
                if (value is Havok.hk2014.hkbCharacterControlCommand.CharacterControlCommand castValue)
                {
                    instance.m_command = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_command = (Havok.hk2014.hkbCharacterControlCommand.CharacterControlCommand)byteValue;
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
