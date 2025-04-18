// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbPoseMatchingGeneratorInternalStateData : HavokData<hkbPoseMatchingGeneratorInternalState> 
{
    public hkbPoseMatchingGeneratorInternalStateData(HavokType type, hkbPoseMatchingGeneratorInternalState instance) : base(type, instance) {}

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
            case "m_currentMatch":
            case "currentMatch":
            {
                if (instance.m_currentMatch is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bestMatch":
            case "bestMatch":
            {
                if (instance.m_bestMatch is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_timeSinceBetterMatch":
            case "timeSinceBetterMatch":
            {
                if (instance.m_timeSinceBetterMatch is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_error":
            case "error":
            {
                if (instance.m_error is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_resetCurrentMatchLocalTime":
            case "resetCurrentMatchLocalTime":
            {
                if (instance.m_resetCurrentMatchLocalTime is not TGet castValue) return false;
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
