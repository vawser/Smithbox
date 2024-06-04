// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class CustomTransitionEffectData : HavokData<CustomTransitionEffect> 
{
    public CustomTransitionEffectData(HavokType type, CustomTransitionEffect instance) : base(type, instance) {}

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
            case "m_selfTransitionMode":
            case "selfTransitionMode":
            {
                if (instance.m_selfTransitionMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_selfTransitionMode is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_eventMode":
            case "eventMode":
            {
                if (instance.m_eventMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_eventMode is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_duration":
            case "duration":
            {
                if (instance.m_duration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toGeneratorStartTimeFraction":
            case "toGeneratorStartTimeFraction":
            {
                if (instance.m_toGeneratorStartTimeFraction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_flags is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_endMode":
            case "endMode":
            {
                if (instance.m_endMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_endMode is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_blendCurve":
            case "blendCurve":
            {
                if (instance.m_blendCurve is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_blendCurve is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_alignmentBone":
            case "alignmentBone":
            {
                if (instance.m_alignmentBone is not TGet castValue) return false;
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
            case "m_selfTransitionMode":
            case "selfTransitionMode":
            {
                if (value is hkbTransitionEffect.SelfTransitionMode castValue)
                {
                    instance.m_selfTransitionMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_selfTransitionMode = (hkbTransitionEffect.SelfTransitionMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_eventMode":
            case "eventMode":
            {
                if (value is hkbTransitionEffect.EventMode castValue)
                {
                    instance.m_eventMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_eventMode = (hkbTransitionEffect.EventMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_duration":
            case "duration":
            {
                if (value is not float castValue) return false;
                instance.m_duration = castValue;
                return true;
            }
            case "m_toGeneratorStartTimeFraction":
            case "toGeneratorStartTimeFraction":
            {
                if (value is not float castValue) return false;
                instance.m_toGeneratorStartTimeFraction = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is hkbBlendingTransitionEffect.FlagBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (hkbBlendingTransitionEffect.FlagBits)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_endMode":
            case "endMode":
            {
                if (value is hkbBlendingTransitionEffect.EndMode castValue)
                {
                    instance.m_endMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_endMode = (hkbBlendingTransitionEffect.EndMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_blendCurve":
            case "blendCurve":
            {
                if (value is hkbBlendCurveUtils.BlendCurve castValue)
                {
                    instance.m_blendCurve = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_blendCurve = (hkbBlendCurveUtils.BlendCurve)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_alignmentBone":
            case "alignmentBone":
            {
                if (value is not short castValue) return false;
                instance.m_alignmentBone = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
