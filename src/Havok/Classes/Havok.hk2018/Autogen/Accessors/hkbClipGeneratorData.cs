// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbClipGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbClipGenerator>
{
    public hkbClipGeneratorData(Havok.hk2018.hkbClipGenerator instance) : base(instance) { }

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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_animationName":
            case "animationName":
            {
                value = instance.m_animationName;
                return true;
            }
            case "m_triggers":
            case "triggers":
            {
                value = instance.m_triggers;
                return true;
            }
            case "m_userPartitionMask":
            case "userPartitionMask":
            {
                value = instance.m_userPartitionMask;
                return true;
            }
            case "m_cropStartAmountLocalTime":
            case "cropStartAmountLocalTime":
            {
                value = instance.m_cropStartAmountLocalTime;
                return true;
            }
            case "m_cropEndAmountLocalTime":
            case "cropEndAmountLocalTime":
            {
                value = instance.m_cropEndAmountLocalTime;
                return true;
            }
            case "m_startTime":
            case "startTime":
            {
                value = instance.m_startTime;
                return true;
            }
            case "m_playbackSpeed":
            case "playbackSpeed":
            {
                value = instance.m_playbackSpeed;
                return true;
            }
            case "m_enforcedDuration":
            case "enforcedDuration":
            {
                value = instance.m_enforcedDuration;
                return true;
            }
            case "m_userControlledTimeFraction":
            case "userControlledTimeFraction":
            {
                value = instance.m_userControlledTimeFraction;
                return true;
            }
            case "m_mode":
            case "mode":
            {
                value = instance.m_mode;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_animationInternalId":
            case "animationInternalId":
            {
                value = instance.m_animationInternalId;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbVariableBindingSet castValue)
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
                if (value is Havok.hk2018.hkbClipTriggerArray castValue)
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
                if (value is Havok.hk2018.hkbClipGenerator.PlaybackMode castValue)
                {
                    instance.m_mode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_mode = (Havok.hk2018.hkbClipGenerator.PlaybackMode)sbyteValue;
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
