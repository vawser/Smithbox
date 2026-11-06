// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbMoveCharacterModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbMoveCharacterModifierInternalState>
{
    public hkbMoveCharacterModifierInternalStateData(Havok.hk2014.hkbMoveCharacterModifierInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_timeSinceLastModify":
            case "timeSinceLastModify":
            {
                value = instance.m_timeSinceLastModify;
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
            case "m_timeSinceLastModify":
            case "timeSinceLastModify":
            {
                if (value is not float castValue) return false;
                instance.m_timeSinceLastModify = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
