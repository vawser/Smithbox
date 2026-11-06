// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class CustomClipGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.CustomClipGenerator>
{
    public CustomClipGeneratorData(Havok.hk2014.CustomClipGenerator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_playbackSpeed":
            case "playbackSpeed":
            {
                value = instance.m_playbackSpeed;
                return true;
            }
            case "m_mode":
            case "mode":
            {
                value = instance.m_mode;
                return true;
            }
            case "m_animId":
            case "animId":
            {
                value = instance.m_animId;
                return true;
            }
            case "m_animeEndEventType":
            case "animeEndEventType":
            {
                value = instance.m_animeEndEventType;
                return true;
            }
            case "m_endEvent":
            case "endEvent":
            {
                value = instance.m_endEvent;
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
            case "m_enforcedDuration":
            case "enforcedDuration":
            {
                value = instance.m_enforcedDuration;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbVariableBindingSet castValue)
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
            case "m_playbackSpeed":
            case "playbackSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_playbackSpeed = castValue;
                return true;
            }
            case "m_mode":
            case "mode":
            {
                if (value is Havok.hk2014.CustomClipGenerator.PlaybackMode castValue)
                {
                    instance.m_mode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_mode = (Havok.hk2014.CustomClipGenerator.PlaybackMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_animId":
            case "animId":
            {
                if (value is not int castValue) return false;
                instance.m_animId = castValue;
                return true;
            }
            case "m_animeEndEventType":
            case "animeEndEventType":
            {
                if (value is Havok.hk2014.CustomClipGenerator.AnimeEndEventType castValue)
                {
                    instance.m_animeEndEventType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_animeEndEventType = (Havok.hk2014.CustomClipGenerator.AnimeEndEventType)intValue;
                    return true;
                }
                return false;
            }
            case "m_endEvent":
            case "endEvent":
            {
                if (value is not Havok.hk2014.hkbEvent castValue) return false;
                instance.m_endEvent = castValue;
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
            case "m_enforcedDuration":
            case "enforcedDuration":
            {
                if (value is not float castValue) return false;
                instance.m_enforcedDuration = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not sbyte castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
