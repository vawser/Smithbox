// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbGeneratorTransitionEffectData : HavokData<hkbGeneratorTransitionEffect> 
{
    public hkbGeneratorTransitionEffectData(HavokType type, hkbGeneratorTransitionEffect instance) : base(type, instance) {}

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
            case "m_transitionGenerator":
            case "transitionGenerator":
            {
                if (instance.m_transitionGenerator is null)
                {
                    return true;
                }
                if (instance.m_transitionGenerator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_blendInDuration":
            case "blendInDuration":
            {
                if (instance.m_blendInDuration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blendOutDuration":
            case "blendOutDuration":
            {
                if (instance.m_blendOutDuration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_syncToGeneratorStartTime":
            case "syncToGeneratorStartTime":
            {
                if (instance.m_syncToGeneratorStartTime is not TGet castValue) return false;
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
            case "m_transitionGenerator":
            case "transitionGenerator":
            {
                if (value is null)
                {
                    instance.m_transitionGenerator = default;
                    return true;
                }
                if (value is hkbGenerator castValue)
                {
                    instance.m_transitionGenerator = castValue;
                    return true;
                }
                return false;
            }
            case "m_blendInDuration":
            case "blendInDuration":
            {
                if (value is not float castValue) return false;
                instance.m_blendInDuration = castValue;
                return true;
            }
            case "m_blendOutDuration":
            case "blendOutDuration":
            {
                if (value is not float castValue) return false;
                instance.m_blendOutDuration = castValue;
                return true;
            }
            case "m_syncToGeneratorStartTime":
            case "syncToGeneratorStartTime":
            {
                if (value is not bool castValue) return false;
                instance.m_syncToGeneratorStartTime = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
