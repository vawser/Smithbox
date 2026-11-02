// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpBodyQualityData : HavokData<HKLib.hk2019.hknpBodyQuality> 
{
    public hknpBodyQualityData(HavokType type, HKLib.hk2019.hknpBodyQuality instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_priority":
            case "priority":
            {
                if (instance.m_priority is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_supportedFlags":
            case "supportedFlags":
            {
                if (instance.m_supportedFlags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_supportedFlags is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_requestedFlags":
            case "requestedFlags":
            {
                if (instance.m_requestedFlags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_requestedFlags is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_contactCachingRelativeMovementThreshold":
            case "contactCachingRelativeMovementThreshold":
            {
                if (instance.m_contactCachingRelativeMovementThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_motionRangeBreachPolicy":
            case "motionRangeBreachPolicy":
            {
                if (instance.m_motionRangeBreachPolicy is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_motionRangeBreachPolicy is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_motionWeldBreachPolicy":
            case "motionWeldBreachPolicy":
            {
                if (instance.m_motionWeldBreachPolicy is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_motionWeldBreachPolicy is TGet intValue)
                {
                    value = intValue;
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
            case "m_priority":
            case "priority":
            {
                if (value is not int castValue) return false;
                instance.m_priority = castValue;
                return true;
            }
            case "m_supportedFlags":
            case "supportedFlags":
            {
                if (value is HKLib.hk2019.hknpBodyQuality.FlagsEnum castValue)
                {
                    instance.m_supportedFlags = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_supportedFlags = (HKLib.hk2019.hknpBodyQuality.FlagsEnum)uintValue;
                    return true;
                }
                return false;
            }
            case "m_requestedFlags":
            case "requestedFlags":
            {
                if (value is HKLib.hk2019.hknpBodyQuality.FlagsEnum castValue)
                {
                    instance.m_requestedFlags = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_requestedFlags = (HKLib.hk2019.hknpBodyQuality.FlagsEnum)uintValue;
                    return true;
                }
                return false;
            }
            case "m_contactCachingRelativeMovementThreshold":
            case "contactCachingRelativeMovementThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_contactCachingRelativeMovementThreshold = castValue;
                return true;
            }
            case "m_motionRangeBreachPolicy":
            case "motionRangeBreachPolicy":
            {
                if (value is HKLib.hk2019.hknpMotionRangeBreachPolicy.Enum castValue)
                {
                    instance.m_motionRangeBreachPolicy = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_motionRangeBreachPolicy = (HKLib.hk2019.hknpMotionRangeBreachPolicy.Enum)intValue;
                    return true;
                }
                return false;
            }
            case "m_motionWeldBreachPolicy":
            case "motionWeldBreachPolicy":
            {
                if (value is HKLib.hk2019.hknpMotionRangeBreachPolicy.Enum castValue)
                {
                    instance.m_motionWeldBreachPolicy = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_motionWeldBreachPolicy = (HKLib.hk2019.hknpMotionRangeBreachPolicy.Enum)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
