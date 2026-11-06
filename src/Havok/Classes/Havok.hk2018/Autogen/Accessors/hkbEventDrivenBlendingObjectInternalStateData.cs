// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbEventDrivenBlendingObjectInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbEventDrivenBlendingObject.InternalState>
{
    public hkbEventDrivenBlendingObjectInternalStateData(Havok.hk2018.hkbEventDrivenBlendingObject.InternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_weight":
            case "weight":
            {
                value = instance.m_weight;
                return true;
            }
            case "m_timeElapsed":
            case "timeElapsed":
            {
                value = instance.m_timeElapsed;
                return true;
            }
            case "m_onFraction":
            case "onFraction":
            {
                value = instance.m_onFraction;
                return true;
            }
            case "m_onFractionOffset":
            case "onFractionOffset":
            {
                value = instance.m_onFractionOffset;
                return true;
            }
            case "m_fadingState":
            case "fadingState":
            {
                value = instance.m_fadingState;
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
            case "m_weight":
            case "weight":
            {
                if (value is not float castValue) return false;
                instance.m_weight = castValue;
                return true;
            }
            case "m_timeElapsed":
            case "timeElapsed":
            {
                if (value is not float castValue) return false;
                instance.m_timeElapsed = castValue;
                return true;
            }
            case "m_onFraction":
            case "onFraction":
            {
                if (value is not float castValue) return false;
                instance.m_onFraction = castValue;
                return true;
            }
            case "m_onFractionOffset":
            case "onFractionOffset":
            {
                if (value is not float castValue) return false;
                instance.m_onFractionOffset = castValue;
                return true;
            }
            case "m_fadingState":
            case "fadingState":
            {
                if (value is Havok.hk2018.hkbEventDrivenBlendingObject.InternalState.FadingState castValue)
                {
                    instance.m_fadingState = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_fadingState = (Havok.hk2018.hkbEventDrivenBlendingObject.InternalState.FadingState)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
