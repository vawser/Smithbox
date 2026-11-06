// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbTimerModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbTimerModifierInternalState>
{
    public hkbTimerModifierInternalStateData(Havok.hk2014.hkbTimerModifierInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_secondsElapsed":
            case "secondsElapsed":
            {
                value = instance.m_secondsElapsed;
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
            case "m_secondsElapsed":
            case "secondsElapsed":
            {
                if (value is not float castValue) return false;
                instance.m_secondsElapsed = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
