// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbAiControlPathToCommandData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbAiControlPathToCommand>
{
    public hkbAiControlPathToCommandData(Havok.hk2014.hkbAiControlPathToCommand instance) : base(instance) { }

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
            case "m_goalPoint":
            case "goalPoint":
            {
                value = instance.m_goalPoint;
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
            case "m_goalPoint":
            case "goalPoint":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_goalPoint = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
