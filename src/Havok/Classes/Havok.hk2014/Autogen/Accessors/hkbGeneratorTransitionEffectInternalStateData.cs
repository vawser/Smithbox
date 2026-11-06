// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbGeneratorTransitionEffectInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbGeneratorTransitionEffectInternalState>
{
    public hkbGeneratorTransitionEffectInternalStateData(Havok.hk2014.hkbGeneratorTransitionEffectInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_timeInTransition":
            case "timeInTransition":
            {
                value = instance.m_timeInTransition;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                value = instance.m_duration;
                return true;
            }
            case "m_effectiveBlendInDuration":
            case "effectiveBlendInDuration":
            {
                value = instance.m_effectiveBlendInDuration;
                return true;
            }
            case "m_effectiveBlendOutDuration":
            case "effectiveBlendOutDuration":
            {
                value = instance.m_effectiveBlendOutDuration;
                return true;
            }
            case "m_toGeneratorState":
            case "toGeneratorState":
            {
                value = instance.m_toGeneratorState;
                return true;
            }
            case "m_echoTransitionGenerator":
            case "echoTransitionGenerator":
            {
                value = instance.m_echoTransitionGenerator;
                return true;
            }
            case "m_toGeneratorSelfTransitionMode":
            case "toGeneratorSelfTransitionMode":
            {
                value = instance.m_toGeneratorSelfTransitionMode;
                return true;
            }
            case "m_justActivated":
            case "justActivated":
            {
                value = instance.m_justActivated;
                return true;
            }
            case "m_updateActiveNodes":
            case "updateActiveNodes":
            {
                value = instance.m_updateActiveNodes;
                return true;
            }
            case "m_stage":
            case "stage":
            {
                value = instance.m_stage;
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
            case "m_timeInTransition":
            case "timeInTransition":
            {
                if (value is not float castValue) return false;
                instance.m_timeInTransition = castValue;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                if (value is not float castValue) return false;
                instance.m_duration = castValue;
                return true;
            }
            case "m_effectiveBlendInDuration":
            case "effectiveBlendInDuration":
            {
                if (value is not float castValue) return false;
                instance.m_effectiveBlendInDuration = castValue;
                return true;
            }
            case "m_effectiveBlendOutDuration":
            case "effectiveBlendOutDuration":
            {
                if (value is not float castValue) return false;
                instance.m_effectiveBlendOutDuration = castValue;
                return true;
            }
            case "m_toGeneratorState":
            case "toGeneratorState":
            {
                if (value is Havok.hk2014.hkbGeneratorTransitionEffect.ToGeneratorState castValue)
                {
                    instance.m_toGeneratorState = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_toGeneratorState = (Havok.hk2014.hkbGeneratorTransitionEffect.ToGeneratorState)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_echoTransitionGenerator":
            case "echoTransitionGenerator":
            {
                if (value is not bool castValue) return false;
                instance.m_echoTransitionGenerator = castValue;
                return true;
            }
            case "m_toGeneratorSelfTransitionMode":
            case "toGeneratorSelfTransitionMode":
            {
                if (value is Havok.hk2014.hkbTransitionEffect.SelfTransitionMode castValue)
                {
                    instance.m_toGeneratorSelfTransitionMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_toGeneratorSelfTransitionMode = (Havok.hk2014.hkbTransitionEffect.SelfTransitionMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_justActivated":
            case "justActivated":
            {
                if (value is not bool castValue) return false;
                instance.m_justActivated = castValue;
                return true;
            }
            case "m_updateActiveNodes":
            case "updateActiveNodes":
            {
                if (value is not bool castValue) return false;
                instance.m_updateActiveNodes = castValue;
                return true;
            }
            case "m_stage":
            case "stage":
            {
                if (value is Havok.hk2014.hkbGeneratorTransitionEffect.Stage castValue)
                {
                    instance.m_stage = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_stage = (Havok.hk2014.hkbGeneratorTransitionEffect.Stage)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
