// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiBlockedCharacterDetectorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiBlockedCharacterDetector>
{
    public hkaiBlockedCharacterDetectorData(Havok.hk2019.hkaiBlockedCharacterDetector instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_smoothingFactor":
            case "smoothingFactor":
            {
                value = instance.m_smoothingFactor;
                return true;
            }
            case "m_initialProgress":
            case "initialProgress":
            {
                value = instance.m_initialProgress;
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
            case "m_hasPrevPos":
            case "hasPrevPos":
            {
                value = instance.m_hasPrevPos;
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
            case "m_smoothingFactor":
            case "smoothingFactor":
            {
                if (value is not float castValue) return false;
                instance.m_smoothingFactor = castValue;
                return true;
            }
            case "m_initialProgress":
            case "initialProgress":
            {
                if (value is not float castValue) return false;
                instance.m_initialProgress = castValue;
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
            case "m_hasPrevPos":
            case "hasPrevPos":
            {
                if (value is not bool castValue) return false;
                instance.m_hasPrevPos = castValue;
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
