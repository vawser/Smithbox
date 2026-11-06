// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbCharacterSelectedInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbCharacterSelectedInfo>
{
    public hkbCharacterSelectedInfoData(Havok.hk2014.hkbCharacterSelectedInfo instance) : base(instance) { }

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
            case "m_scriptDebuggingPort":
            case "scriptDebuggingPort":
            {
                value = instance.m_scriptDebuggingPort;
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
            case "m_scriptDebuggingPort":
            case "scriptDebuggingPort":
            {
                if (value is not int castValue) return false;
                instance.m_scriptDebuggingPort = castValue;
                return true;
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
