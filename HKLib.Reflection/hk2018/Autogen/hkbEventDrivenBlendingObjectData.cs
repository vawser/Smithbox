// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbEventDrivenBlendingObjectData : HavokData<hkbEventDrivenBlendingObject> 
{
    public hkbEventDrivenBlendingObjectData(HavokType type, hkbEventDrivenBlendingObject instance) : base(type, instance) {}

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
            case "m_fadeInDuration":
            case "fadeInDuration":
            {
                if (instance.m_fadeInDuration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                if (instance.m_fadeOutDuration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_onEventId":
            case "onEventId":
            {
                if (instance.m_onEventId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_offEventId":
            case "offEventId":
            {
                if (instance.m_offEventId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_onByDefault":
            case "onByDefault":
            {
                if (instance.m_onByDefault is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_forceFullFadeDurations":
            case "forceFullFadeDurations":
            {
                if (instance.m_forceFullFadeDurations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fadeInOutCurve":
            case "fadeInOutCurve":
            {
                if (instance.m_fadeInOutCurve is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_fadeInOutCurve is TGet sbyteValue)
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
            case "m_fadeInDuration":
            case "fadeInDuration":
            {
                if (value is not float castValue) return false;
                instance.m_fadeInDuration = castValue;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                if (value is not float castValue) return false;
                instance.m_fadeOutDuration = castValue;
                return true;
            }
            case "m_onEventId":
            case "onEventId":
            {
                if (value is not int castValue) return false;
                instance.m_onEventId = castValue;
                return true;
            }
            case "m_offEventId":
            case "offEventId":
            {
                if (value is not int castValue) return false;
                instance.m_offEventId = castValue;
                return true;
            }
            case "m_onByDefault":
            case "onByDefault":
            {
                if (value is not bool castValue) return false;
                instance.m_onByDefault = castValue;
                return true;
            }
            case "m_forceFullFadeDurations":
            case "forceFullFadeDurations":
            {
                if (value is not bool castValue) return false;
                instance.m_forceFullFadeDurations = castValue;
                return true;
            }
            case "m_fadeInOutCurve":
            case "fadeInOutCurve":
            {
                if (value is hkbBlendCurveUtils.BlendCurve castValue)
                {
                    instance.m_fadeInOutCurve = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_fadeInOutCurve = (hkbBlendCurveUtils.BlendCurve)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
