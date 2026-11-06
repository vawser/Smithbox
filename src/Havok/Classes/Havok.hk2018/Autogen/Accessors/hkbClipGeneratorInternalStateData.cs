// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbClipGeneratorInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbClipGeneratorInternalState>
{
    public hkbClipGeneratorInternalStateData(Havok.hk2018.hkbClipGeneratorInternalState instance) : base(instance) { }

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
            case "m_extractedMotion":
            case "extractedMotion":
            {
                value = instance.m_extractedMotion;
                return true;
            }
            case "m_echos":
            case "echos":
            {
                value = instance.m_echos;
                return true;
            }
            case "m_localTime":
            case "localTime":
            {
                value = instance.m_localTime;
                return true;
            }
            case "m_time":
            case "time":
            {
                value = instance.m_time;
                return true;
            }
            case "m_previousUserControlledTimeFraction":
            case "previousUserControlledTimeFraction":
            {
                value = instance.m_previousUserControlledTimeFraction;
                return true;
            }
            case "m_bufferSize":
            case "bufferSize":
            {
                value = instance.m_bufferSize;
                return true;
            }
            case "m_atEnd":
            case "atEnd":
            {
                value = instance.m_atEnd;
                return true;
            }
            case "m_ignoreStartTime":
            case "ignoreStartTime":
            {
                value = instance.m_ignoreStartTime;
                return true;
            }
            case "m_pingPongBackward":
            case "pingPongBackward":
            {
                value = instance.m_pingPongBackward;
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
            case "m_extractedMotion":
            case "extractedMotion":
            {
                if (value is not Havok.hk2018.hkQsTransform castValue) return false;
                instance.m_extractedMotion = castValue;
                return true;
            }
            case "m_echos":
            case "echos":
            {
                if (value is not List<Havok.hk2018.hkbClipGenerator.Echo> castValue) return false;
                instance.m_echos = castValue;
                return true;
            }
            case "m_localTime":
            case "localTime":
            {
                if (value is not float castValue) return false;
                instance.m_localTime = castValue;
                return true;
            }
            case "m_time":
            case "time":
            {
                if (value is not float castValue) return false;
                instance.m_time = castValue;
                return true;
            }
            case "m_previousUserControlledTimeFraction":
            case "previousUserControlledTimeFraction":
            {
                if (value is not float castValue) return false;
                instance.m_previousUserControlledTimeFraction = castValue;
                return true;
            }
            case "m_bufferSize":
            case "bufferSize":
            {
                if (value is not int castValue) return false;
                instance.m_bufferSize = castValue;
                return true;
            }
            case "m_atEnd":
            case "atEnd":
            {
                if (value is not bool castValue) return false;
                instance.m_atEnd = castValue;
                return true;
            }
            case "m_ignoreStartTime":
            case "ignoreStartTime":
            {
                if (value is not bool castValue) return false;
                instance.m_ignoreStartTime = castValue;
                return true;
            }
            case "m_pingPongBackward":
            case "pingPongBackward":
            {
                if (value is not bool castValue) return false;
                instance.m_pingPongBackward = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
