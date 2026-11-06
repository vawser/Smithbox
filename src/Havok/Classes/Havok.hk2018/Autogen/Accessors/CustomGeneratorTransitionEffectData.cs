// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class CustomGeneratorTransitionEffectData : Havok.Formats.HavokFieldAccess<Havok.hk2018.CustomGeneratorTransitionEffect>
{
    public CustomGeneratorTransitionEffectData(Havok.hk2018.CustomGeneratorTransitionEffect instance) : base(instance) { }

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
            case "m_transitionGenerator":
            case "transitionGenerator":
            {
                value = instance.m_transitionGenerator;
                return true;
            }
            case "m_blendInDuration":
            case "blendInDuration":
            {
                value = instance.m_blendInDuration;
                return true;
            }
            case "m_blendOutDuration":
            case "blendOutDuration":
            {
                value = instance.m_blendOutDuration;
                return true;
            }
            case "m_syncToGeneratorStartTime":
            case "syncToGeneratorStartTime":
            {
                value = instance.m_syncToGeneratorStartTime;
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
            case "m_selfTransitionMode":
            case "selfTransitionMode":
            {
                if (value is Havok.hk2018.hkbTransitionEffect.SelfTransitionMode castValue)
                {
                    instance.m_selfTransitionMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_selfTransitionMode = (Havok.hk2018.hkbTransitionEffect.SelfTransitionMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_eventMode":
            case "eventMode":
            {
                if (value is Havok.hk2018.hkbTransitionEffect.EventMode castValue)
                {
                    instance.m_eventMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_eventMode = (Havok.hk2018.hkbTransitionEffect.EventMode)sbyteValue;
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
                if (value is Havok.hk2018.hkbGenerator castValue)
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
