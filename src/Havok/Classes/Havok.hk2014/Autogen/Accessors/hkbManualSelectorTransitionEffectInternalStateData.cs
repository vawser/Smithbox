// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbManualSelectorTransitionEffectInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbManualSelectorTransitionEffectInternalState>
{
    public hkbManualSelectorTransitionEffectInternalStateData(Havok.hk2014.hkbManualSelectorTransitionEffectInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_currentTransitionEffectIndex":
            case "currentTransitionEffectIndex":
            {
                value = instance.m_currentTransitionEffectIndex;
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
            case "m_currentTransitionEffectIndex":
            case "currentTransitionEffectIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_currentTransitionEffectIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
