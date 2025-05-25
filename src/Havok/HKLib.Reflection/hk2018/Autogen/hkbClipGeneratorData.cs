// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbClipGeneratorData : HavokData<hkbClipGenerator> 
{
    public hkbClipGeneratorData(HavokType type, hkbClipGenerator instance) : base(type, instance) {}

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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (instance.m_variableBindingSet is null)
                {
                    return true;
                }
                if (instance.m_variableBindingSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_animationName":
            case "animationName":
            {
                if (instance.m_animationName is null)
                {
                    return true;
                }
                if (instance.m_animationName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_triggers":
            case "triggers":
            {
                if (instance.m_triggers is null)
                {
                    return true;
                }
                if (instance.m_triggers is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_userPartitionMask":
            case "userPartitionMask":
            {
                if (instance.m_userPartitionMask is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cropStartAmountLocalTime":
            case "cropStartAmountLocalTime":
            {
                if (instance.m_cropStartAmountLocalTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cropEndAmountLocalTime":
            case "cropEndAmountLocalTime":
            {
                if (instance.m_cropEndAmountLocalTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startTime":
            case "startTime":
            {
                if (instance.m_startTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_playbackSpeed":
            case "playbackSpeed":
            {
                if (instance.m_playbackSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enforcedDuration":
            case "enforcedDuration":
            {
                if (instance.m_enforcedDuration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userControlledTimeFraction":
            case "userControlledTimeFraction":
            {
                if (instance.m_userControlledTimeFraction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mode":
            case "mode":
            {
                if (instance.m_mode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_mode is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_animationInternalId":
            case "animationInternalId":
            {
                if (instance.m_animationInternalId is not TGet castValue) return false;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_animationName":
            case "animationName":
            {
                if (value is null)
                {
                    instance.m_animationName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_animationName = castValue;
                    return true;
                }
                return false;
            }
            case "m_triggers":
            case "triggers":
            {
                if (value is null)
                {
                    instance.m_triggers = default;
                    return true;
                }
                if (value is hkbClipTriggerArray castValue)
                {
                    instance.m_triggers = castValue;
                    return true;
                }
                return false;
            }
            case "m_userPartitionMask":
            case "userPartitionMask":
            {
                if (value is not uint castValue) return false;
                instance.m_userPartitionMask = castValue;
                return true;
            }
            case "m_cropStartAmountLocalTime":
            case "cropStartAmountLocalTime":
            {
                if (value is not float castValue) return false;
                instance.m_cropStartAmountLocalTime = castValue;
                return true;
            }
            case "m_cropEndAmountLocalTime":
            case "cropEndAmountLocalTime":
            {
                if (value is not float castValue) return false;
                instance.m_cropEndAmountLocalTime = castValue;
                return true;
            }
            case "m_startTime":
            case "startTime":
            {
                if (value is not float castValue) return false;
                instance.m_startTime = castValue;
                return true;
            }
            case "m_playbackSpeed":
            case "playbackSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_playbackSpeed = castValue;
                return true;
            }
            case "m_enforcedDuration":
            case "enforcedDuration":
            {
                if (value is not float castValue) return false;
                instance.m_enforcedDuration = castValue;
                return true;
            }
            case "m_userControlledTimeFraction":
            case "userControlledTimeFraction":
            {
                if (value is not float castValue) return false;
                instance.m_userControlledTimeFraction = castValue;
                return true;
            }
            case "m_mode":
            case "mode":
            {
                if (value is hkbClipGenerator.PlaybackMode castValue)
                {
                    instance.m_mode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_mode = (hkbClipGenerator.PlaybackMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not sbyte castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            case "m_animationInternalId":
            case "animationInternalId":
            {
                if (value is not short castValue) return false;
                instance.m_animationInternalId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
