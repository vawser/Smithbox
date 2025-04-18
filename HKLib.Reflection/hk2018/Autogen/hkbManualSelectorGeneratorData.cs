// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbManualSelectorGeneratorData : HavokData<hkbManualSelectorGenerator> 
{
    public hkbManualSelectorGeneratorData(HavokType type, hkbManualSelectorGenerator instance) : base(type, instance) {}

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
            case "m_generators":
            case "generators":
            {
                if (instance.m_generators is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_selectedGeneratorIndex":
            case "selectedGeneratorIndex":
            {
                if (instance.m_selectedGeneratorIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_indexSelector":
            case "indexSelector":
            {
                if (instance.m_indexSelector is null)
                {
                    return true;
                }
                if (instance.m_indexSelector is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_selectedIndexCanChangeAfterActivate":
            case "selectedIndexCanChangeAfterActivate":
            {
                if (instance.m_selectedIndexCanChangeAfterActivate is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_generatorChangedTransitionEffect":
            case "generatorChangedTransitionEffect":
            {
                if (instance.m_generatorChangedTransitionEffect is null)
                {
                    return true;
                }
                if (instance.m_generatorChangedTransitionEffect is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_sentOnClipEnd":
            case "sentOnClipEnd":
            {
                if (instance.m_sentOnClipEnd is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_generatorPreDeleteIndex":
            case "generatorPreDeleteIndex":
            {
                if (instance.m_generatorPreDeleteIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_endOfClipEventId":
            case "endOfClipEventId":
            {
                if (instance.m_endOfClipEventId is not TGet castValue) return false;
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
            case "m_generators":
            case "generators":
            {
                if (value is not List<hkbGenerator?> castValue) return false;
                instance.m_generators = castValue;
                return true;
            }
            case "m_selectedGeneratorIndex":
            case "selectedGeneratorIndex":
            {
                if (value is not short castValue) return false;
                instance.m_selectedGeneratorIndex = castValue;
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
                if (value is hkbCustomIdSelector castValue)
                {
                    instance.m_indexSelector = castValue;
                    return true;
                }
                return false;
            }
            case "m_selectedIndexCanChangeAfterActivate":
            case "selectedIndexCanChangeAfterActivate":
            {
                if (value is not bool castValue) return false;
                instance.m_selectedIndexCanChangeAfterActivate = castValue;
                return true;
            }
            case "m_generatorChangedTransitionEffect":
            case "generatorChangedTransitionEffect":
            {
                if (value is null)
                {
                    instance.m_generatorChangedTransitionEffect = default;
                    return true;
                }
                if (value is hkbTransitionEffect castValue)
                {
                    instance.m_generatorChangedTransitionEffect = castValue;
                    return true;
                }
                return false;
            }
            case "m_sentOnClipEnd":
            case "sentOnClipEnd":
            {
                if (value is not hkbEventProperty castValue) return false;
                instance.m_sentOnClipEnd = castValue;
                return true;
            }
            case "m_generatorPreDeleteIndex":
            case "generatorPreDeleteIndex":
            {
                if (value is not List<short> castValue) return false;
                instance.m_generatorPreDeleteIndex = castValue;
                return true;
            }
            case "m_endOfClipEventId":
            case "endOfClipEventId":
            {
                if (value is not int castValue) return false;
                instance.m_endOfClipEventId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
