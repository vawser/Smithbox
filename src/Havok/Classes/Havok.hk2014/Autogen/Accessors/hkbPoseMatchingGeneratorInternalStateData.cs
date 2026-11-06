// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbPoseMatchingGeneratorInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbPoseMatchingGeneratorInternalState>
{
    public hkbPoseMatchingGeneratorInternalStateData(Havok.hk2014.hkbPoseMatchingGeneratorInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_currentMatch":
            case "currentMatch":
            {
                value = instance.m_currentMatch;
                return true;
            }
            case "m_bestMatch":
            case "bestMatch":
            {
                value = instance.m_bestMatch;
                return true;
            }
            case "m_timeSinceBetterMatch":
            case "timeSinceBetterMatch":
            {
                value = instance.m_timeSinceBetterMatch;
                return true;
            }
            case "m_error":
            case "error":
            {
                value = instance.m_error;
                return true;
            }
            case "m_resetCurrentMatchLocalTime":
            case "resetCurrentMatchLocalTime":
            {
                value = instance.m_resetCurrentMatchLocalTime;
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
            case "m_currentMatch":
            case "currentMatch":
            {
                if (value is not int castValue) return false;
                instance.m_currentMatch = castValue;
                return true;
            }
            case "m_bestMatch":
            case "bestMatch":
            {
                if (value is not int castValue) return false;
                instance.m_bestMatch = castValue;
                return true;
            }
            case "m_timeSinceBetterMatch":
            case "timeSinceBetterMatch":
            {
                if (value is not float castValue) return false;
                instance.m_timeSinceBetterMatch = castValue;
                return true;
            }
            case "m_error":
            case "error":
            {
                if (value is not float castValue) return false;
                instance.m_error = castValue;
                return true;
            }
            case "m_resetCurrentMatchLocalTime":
            case "resetCurrentMatchLocalTime":
            {
                if (value is not bool castValue) return false;
                instance.m_resetCurrentMatchLocalTime = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
