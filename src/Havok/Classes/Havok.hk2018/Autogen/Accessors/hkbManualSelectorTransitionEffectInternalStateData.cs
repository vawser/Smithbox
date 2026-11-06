// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbManualSelectorTransitionEffectInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbManualSelectorTransitionEffectInternalState>
{
    public hkbManualSelectorTransitionEffectInternalStateData(Havok.hk2018.hkbManualSelectorTransitionEffectInternalState instance) : base(instance) { }

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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
