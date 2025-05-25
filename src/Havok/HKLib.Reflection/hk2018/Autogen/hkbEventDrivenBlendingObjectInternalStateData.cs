// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbEventDrivenBlendingObjectInternalStateData : HavokData<hkbEventDrivenBlendingObject.InternalState> 
{
    public hkbEventDrivenBlendingObjectInternalStateData(HavokType type, hkbEventDrivenBlendingObject.InternalState instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_weight":
            case "weight":
            {
                if (instance.m_weight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_timeElapsed":
            case "timeElapsed":
            {
                if (instance.m_timeElapsed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_onFraction":
            case "onFraction":
            {
                if (instance.m_onFraction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_onFractionOffset":
            case "onFractionOffset":
            {
                if (instance.m_onFractionOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fadingState":
            case "fadingState":
            {
                if (instance.m_fadingState is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_fadingState is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is hkbEventDrivenBlendingObject.InternalState.FadingState castValue)
                {
                    instance.m_fadingState = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_fadingState = (hkbEventDrivenBlendingObject.InternalState.FadingState)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
