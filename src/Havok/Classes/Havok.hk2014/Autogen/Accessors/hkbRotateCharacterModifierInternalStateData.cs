// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbRotateCharacterModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbRotateCharacterModifierInternalState>
{
    public hkbRotateCharacterModifierInternalStateData(Havok.hk2014.hkbRotateCharacterModifierInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_angle":
            case "angle":
            {
                value = instance.m_angle;
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
            case "m_angle":
            case "angle":
            {
                if (value is not float castValue) return false;
                instance.m_angle = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
