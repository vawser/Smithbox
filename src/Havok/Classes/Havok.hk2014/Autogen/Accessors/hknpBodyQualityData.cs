// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpBodyQualityData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpBodyQuality>
{
    public hknpBodyQualityData(Havok.hk2014.hknpBodyQuality instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_priority":
            case "priority":
            {
                value = instance.m_priority;
                return true;
            }
            case "m_supportedFlags":
            case "supportedFlags":
            {
                value = instance.m_supportedFlags;
                return true;
            }
            case "m_requestedFlags":
            case "requestedFlags":
            {
                value = instance.m_requestedFlags;
                return true;
            }
            case "m_contactCachingRelativeMovementThreshold":
            case "contactCachingRelativeMovementThreshold":
            {
                value = instance.m_contactCachingRelativeMovementThreshold;
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
                if (value is Havok.hk2014.hknpBodyQuality.FlagsEnum castValue)
                {
                    instance.m_supportedFlags = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_supportedFlags = (Havok.hk2014.hknpBodyQuality.FlagsEnum)uintValue;
                    return true;
                }
                return false;
            }
            case "m_requestedFlags":
            case "requestedFlags":
            {
                if (value is Havok.hk2014.hknpBodyQuality.FlagsEnum castValue)
                {
                    instance.m_requestedFlags = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_requestedFlags = (Havok.hk2014.hknpBodyQuality.FlagsEnum)uintValue;
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
            default:
            return false;
        }
    }

}
