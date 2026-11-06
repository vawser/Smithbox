// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiBehaviorBlockedDetectorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiBehaviorBlockedDetector>
{
    public hkaiBehaviorBlockedDetectorData(Havok.hk2018.hkaiBehaviorBlockedDetector instance) : base(instance) { }

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
            case "m_behavior":
            case "behavior":
            {
                value = instance.m_behavior;
                return true;
            }
            case "m_prevRuntimeID":
            case "prevRuntimeID":
            {
                value = instance.m_prevRuntimeID;
                return true;
            }
            case "m_prevPos":
            case "prevPos":
            {
                value = instance.m_prevPos;
                return true;
            }
            case "m_avgProgress":
            case "avgProgress":
            {
                value = instance.m_avgProgress;
                return true;
            }
            case "m_smoothingFactor":
            case "smoothingFactor":
            {
                value = instance.m_smoothingFactor;
                return true;
            }
            case "m_blockedThreshold":
            case "blockedThreshold":
            {
                value = instance.m_blockedThreshold;
                return true;
            }
            case "m_sqrTeleportationThreshold":
            case "sqrTeleportationThreshold":
            {
                value = instance.m_sqrTeleportationThreshold;
                return true;
            }
            case "m_blocked":
            case "blocked":
            {
                value = instance.m_blocked;
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
            case "m_behavior":
            case "behavior":
            {
                if (value is null)
                {
                    instance.m_behavior = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiEdgeFollowingBehavior castValue)
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
