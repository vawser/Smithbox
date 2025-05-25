// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbClipGeneratorInternalStateData : HavokData<hkbClipGeneratorInternalState> 
{
    public hkbClipGeneratorInternalStateData(HavokType type, hkbClipGeneratorInternalState instance) : base(type, instance) {}

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
            case "m_extractedMotion":
            case "extractedMotion":
            {
                if (instance.m_extractedMotion is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_echos":
            case "echos":
            {
                if (instance.m_echos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localTime":
            case "localTime":
            {
                if (instance.m_localTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_time":
            case "time":
            {
                if (instance.m_time is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_previousUserControlledTimeFraction":
            case "previousUserControlledTimeFraction":
            {
                if (instance.m_previousUserControlledTimeFraction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bufferSize":
            case "bufferSize":
            {
                if (instance.m_bufferSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_atEnd":
            case "atEnd":
            {
                if (instance.m_atEnd is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ignoreStartTime":
            case "ignoreStartTime":
            {
                if (instance.m_ignoreStartTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pingPongBackward":
            case "pingPongBackward":
            {
                if (instance.m_pingPongBackward is not TGet castValue) return false;
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
            case "m_extractedMotion":
            case "extractedMotion":
            {
                if (value is not hkQsTransform castValue) return false;
                instance.m_extractedMotion = castValue;
                return true;
            }
            case "m_echos":
            case "echos":
            {
                if (value is not List<hkbClipGenerator.Echo> castValue) return false;
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
