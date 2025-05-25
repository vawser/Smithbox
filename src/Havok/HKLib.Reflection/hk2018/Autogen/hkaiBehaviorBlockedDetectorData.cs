// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiBehaviorBlockedDetectorData : HavokData<hkaiBehaviorBlockedDetector> 
{
    public hkaiBehaviorBlockedDetectorData(HavokType type, hkaiBehaviorBlockedDetector instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_behavior":
            case "behavior":
            {
                if (instance.m_behavior is null)
                {
                    return true;
                }
                if (instance.m_behavior is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_prevRuntimeID":
            case "prevRuntimeID":
            {
                if (instance.m_prevRuntimeID is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_prevPos":
            case "prevPos":
            {
                if (instance.m_prevPos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_avgProgress":
            case "avgProgress":
            {
                if (instance.m_avgProgress is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_smoothingFactor":
            case "smoothingFactor":
            {
                if (instance.m_smoothingFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blockedThreshold":
            case "blockedThreshold":
            {
                if (instance.m_blockedThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sqrTeleportationThreshold":
            case "sqrTeleportationThreshold":
            {
                if (instance.m_sqrTeleportationThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blocked":
            case "blocked":
            {
                if (instance.m_blocked is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_behavior":
            case "behavior":
            {
                if (value is null)
                {
                    instance.m_behavior = default;
                    return true;
                }
                if (value is hkaiEdgeFollowingBehavior castValue)
                {
                    instance.m_behavior = castValue;
                    return true;
                }
                return false;
            }
            case "m_prevRuntimeID":
            case "prevRuntimeID":
            {
                if (value is not int castValue) return false;
                instance.m_prevRuntimeID = castValue;
                return true;
            }
            case "m_prevPos":
            case "prevPos":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_prevPos = castValue;
                return true;
            }
            case "m_avgProgress":
            case "avgProgress":
            {
                if (value is not float castValue) return false;
                instance.m_avgProgress = castValue;
                return true;
            }
            case "m_smoothingFactor":
            case "smoothingFactor":
            {
                if (value is not float castValue) return false;
                instance.m_smoothingFactor = castValue;
                return true;
            }
            case "m_blockedThreshold":
            case "blockedThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_blockedThreshold = castValue;
                return true;
            }
            case "m_sqrTeleportationThreshold":
            case "sqrTeleportationThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_sqrTeleportationThreshold = castValue;
                return true;
            }
            case "m_blocked":
            case "blocked":
            {
                if (value is not bool castValue) return false;
                instance.m_blocked = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
