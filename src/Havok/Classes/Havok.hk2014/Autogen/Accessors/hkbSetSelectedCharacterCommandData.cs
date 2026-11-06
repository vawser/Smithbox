// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbSetSelectedCharacterCommandData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbSetSelectedCharacterCommand>
{
    public hkbSetSelectedCharacterCommandData(Havok.hk2014.hkbSetSelectedCharacterCommand instance) : base(instance) { }

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
            default:
            return false;
        }
    }

}
