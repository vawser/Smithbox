// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class CustomTransitionEffectData : Havok.Formats.HavokFieldAccess<Havok.hk2016.CustomTransitionEffect>
{
    public CustomTransitionEffectData(Havok.hk2016.CustomTransitionEffect instance) : base(instance) { }

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
            case "m_selfTransitionMode":
            case "selfTransitionMode":
            {
                value = instance.m_selfTransitionMode;
                return true;
            }
            case "m_eventMode":
            case "eventMode":
            {
                value = instance.m_eventMode;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                value = instance.m_duration;
                return true;
            }
            case "m_toGeneratorStartTimeFraction":
            case "toGeneratorStartTimeFraction":
            {
                value = instance.m_toGeneratorStartTimeFraction;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_endMode":
            case "endMode":
            {
                value = instance.m_endMode;
                return true;
            }
            case "m_blendCurve":
            case "blendCurve":
            {
                value = instance.m_blendCurve;
                return true;
            }
            case "m_alignmentBone":
            case "alignmentBone":
            {
                value = instance.m_alignmentBone;
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
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
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
                if (value is Havok.hk2016.hkbVariableBindingSet castValue)
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
                if (value is Havok.hk2016.hkbTransitionEffect.SelfTransitionMode castValue)
                {
                    instance.m_selfTransitionMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_selfTransitionMode = (Havok.hk2016.hkbTransitionEffect.SelfTransitionMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_eventMode":
            case "eventMode":
            {
                if (value is Havok.hk2016.hkbTransitionEffect.EventMode castValue)
                {
                    instance.m_eventMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_eventMode = (Havok.hk2016.hkbTransitionEffect.EventMode)sbyteValue;
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
                if (value is Havok.hk2016.hkbBlendingTransitionEffect.FlagBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (Havok.hk2016.hkbBlendingTransitionEffect.FlagBits)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_endMode":
            case "endMode":
            {
                if (value is Havok.hk2016.hkbBlendingTransitionEffect.EndMode castValue)
                {
                    instance.m_endMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_endMode = (Havok.hk2016.hkbBlendingTransitionEffect.EndMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_blendCurve":
            case "blendCurve":
            {
                if (value is Havok.hk2016.hkbBlendCurveUtils.BlendCurve castValue)
                {
                    instance.m_blendCurve = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_blendCurve = (Havok.hk2016.hkbBlendCurveUtils.BlendCurve)sbyteValue;
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
