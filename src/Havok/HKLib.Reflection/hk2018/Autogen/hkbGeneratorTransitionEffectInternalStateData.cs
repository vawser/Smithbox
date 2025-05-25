// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbGeneratorTransitionEffectInternalStateData : HavokData<hkbGeneratorTransitionEffectInternalState> 
{
    public hkbGeneratorTransitionEffectInternalStateData(HavokType type, hkbGeneratorTransitionEffectInternalState instance) : base(type, instance) {}

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
            case "m_timeInTransition":
            case "timeInTransition":
            {
                if (instance.m_timeInTransition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                if (instance.m_duration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_effectiveBlendInDuration":
            case "effectiveBlendInDuration":
            {
                if (instance.m_effectiveBlendInDuration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_effectiveBlendOutDuration":
            case "effectiveBlendOutDuration":
            {
                if (instance.m_effectiveBlendOutDuration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toGeneratorState":
            case "toGeneratorState":
            {
                if (instance.m_toGeneratorState is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_toGeneratorState is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_echoTransitionGenerator":
            case "echoTransitionGenerator":
            {
                if (instance.m_echoTransitionGenerator is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toGeneratorSelfTransitionMode":
            case "toGeneratorSelfTransitionMode":
            {
                if (instance.m_toGeneratorSelfTransitionMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_toGeneratorSelfTransitionMode is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_justActivated":
            case "justActivated":
            {
                if (instance.m_justActivated is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_updateActiveNodes":
            case "updateActiveNodes":
            {
                if (instance.m_updateActiveNodes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stage":
            case "stage":
            {
                if (instance.m_stage is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_stage is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
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
                if (value is hkbGeneratorTransitionEffect.ToGeneratorState castValue)
                {
                    instance.m_toGeneratorState = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_toGeneratorState = (hkbGeneratorTransitionEffect.ToGeneratorState)sbyteValue;
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
                if (value is hkbTransitionEffect.SelfTransitionMode castValue)
                {
                    instance.m_toGeneratorSelfTransitionMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_toGeneratorSelfTransitionMode = (hkbTransitionEffect.SelfTransitionMode)sbyteValue;
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
                if (value is hkbGeneratorTransitionEffect.Stage castValue)
                {
                    instance.m_stage = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_stage = (hkbGeneratorTransitionEffect.Stage)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
