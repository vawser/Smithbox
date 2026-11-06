// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbManualSelectorTransitionEffectData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbManualSelectorTransitionEffect>
{
    public hkbManualSelectorTransitionEffectData(Havok.hk2016.hkbManualSelectorTransitionEffect instance) : base(instance) { }

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
            case "m_transitionEffects":
            case "transitionEffects":
            {
                value = instance.m_transitionEffects;
                return true;
            }
            case "m_selectedIndex":
            case "selectedIndex":
            {
                value = instance.m_selectedIndex;
                return true;
            }
            case "m_indexSelector":
            case "indexSelector":
            {
                value = instance.m_indexSelector;
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
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
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
            case "m_transitionEffects":
            case "transitionEffects":
            {
                if (value is not List<Havok.hk2016.hkbTransitionEffect?> castValue) return false;
                instance.m_transitionEffects = castValue;
                return true;
            }
            case "m_selectedIndex":
            case "selectedIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_selectedIndex = castValue;
                return true;
            }
            case "m_indexSelector":
            case "indexSelector":
            {
                if (value is null)
                {
                    instance.m_indexSelector = default;
                    return true;
                }
                if (value is Havok.hk2016.hkbCustomIdSelector castValue)
                {
                    instance.m_indexSelector = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
