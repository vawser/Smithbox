// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbManualSelectorGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbManualSelectorGenerator>
{
    public hkbManualSelectorGeneratorData(Havok.hk2014.hkbManualSelectorGenerator instance) : base(instance) { }

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
            case "m_generators":
            case "generators":
            {
                value = instance.m_generators;
                return true;
            }
            case "m_selectedGeneratorIndex":
            case "selectedGeneratorIndex":
            {
                value = instance.m_selectedGeneratorIndex;
                return true;
            }
            case "m_indexSelector":
            case "indexSelector":
            {
                value = instance.m_indexSelector;
                return true;
            }
            case "m_selectedIndexCanChangeAfterActivate":
            case "selectedIndexCanChangeAfterActivate":
            {
                value = instance.m_selectedIndexCanChangeAfterActivate;
                return true;
            }
            case "m_generatorChangedTransitionEffect":
            case "generatorChangedTransitionEffect":
            {
                value = instance.m_generatorChangedTransitionEffect;
                return true;
            }
            case "m_endOfClipEventId":
            case "endOfClipEventId":
            {
                value = instance.m_endOfClipEventId;
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
            case "m_generators":
            case "generators":
            {
                if (value is not List<Havok.hk2014.hkbGenerator?> castValue) return false;
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
                if (value is Havok.hk2014.hkbCustomIdSelector castValue)
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
                if (value is Havok.hk2014.hkbTransitionEffect castValue)
                {
                    instance.m_generatorChangedTransitionEffect = castValue;
                    return true;
                }
                return false;
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
