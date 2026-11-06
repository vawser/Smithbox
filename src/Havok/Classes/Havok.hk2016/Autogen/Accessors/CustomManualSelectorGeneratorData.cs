// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class CustomManualSelectorGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2016.CustomManualSelectorGenerator>
{
    public CustomManualSelectorGeneratorData(Havok.hk2016.CustomManualSelectorGenerator instance) : base(instance) { }

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
            case "m_generators":
            case "generators":
            {
                value = instance.m_generators;
                return true;
            }
            case "m_offsetType":
            case "offsetType":
            {
                value = instance.m_offsetType;
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
            case "m_enableScript":
            case "enableScript":
            {
                value = instance.m_enableScript;
                return true;
            }
            case "m_enableTae":
            case "enableTae":
            {
                value = instance.m_enableTae;
                return true;
            }
            case "m_changeTypeOfSelectedIndexAfterActivate":
            case "changeTypeOfSelectedIndexAfterActivate":
            {
                value = instance.m_changeTypeOfSelectedIndexAfterActivate;
                return true;
            }
            case "m_generatorChangedTransitionEffect":
            case "generatorChangedTransitionEffect":
            {
                value = instance.m_generatorChangedTransitionEffect;
                return true;
            }
            case "m_checkAnimEndSlotNo":
            case "checkAnimEndSlotNo":
            {
                value = instance.m_checkAnimEndSlotNo;
                return true;
            }
            case "m_replanningAI":
            case "replanningAI":
            {
                value = instance.m_replanningAI;
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
            case "m_generators":
            case "generators":
            {
                if (value is not List<Havok.hk2016.hkbGenerator?> castValue) return false;
                instance.m_generators = castValue;
                return true;
            }
            case "m_offsetType":
            case "offsetType":
            {
                if (value is Havok.hk2016.CustomManualSelectorGenerator.OffsetType castValue)
                {
                    instance.m_offsetType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_offsetType = (Havok.hk2016.CustomManualSelectorGenerator.OffsetType)intValue;
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
                if (value is Havok.hk2016.CustomManualSelectorGenerator.AnimeEndEventType castValue)
                {
                    instance.m_animeEndEventType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_animeEndEventType = (Havok.hk2016.CustomManualSelectorGenerator.AnimeEndEventType)intValue;
                    return true;
                }
                return false;
            }
            case "m_enableScript":
            case "enableScript":
            {
                if (value is not bool castValue) return false;
                instance.m_enableScript = castValue;
                return true;
            }
            case "m_enableTae":
            case "enableTae":
            {
                if (value is not bool castValue) return false;
                instance.m_enableTae = castValue;
                return true;
            }
            case "m_changeTypeOfSelectedIndexAfterActivate":
            case "changeTypeOfSelectedIndexAfterActivate":
            {
                if (value is Havok.hk2016.CustomManualSelectorGenerator.ChangeTypeOfSelectedIndexAfterActivate castValue)
                {
                    instance.m_changeTypeOfSelectedIndexAfterActivate = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_changeTypeOfSelectedIndexAfterActivate = (Havok.hk2016.CustomManualSelectorGenerator.ChangeTypeOfSelectedIndexAfterActivate)byteValue;
                    return true;
                }
                return false;
            }
            case "m_generatorChangedTransitionEffect":
            case "generatorChangedTransitionEffect":
            {
                if (value is null)
                {
                    instance.m_generatorChangedTransitionEffect = default;
                    return true;
                }
                if (value is Havok.hk2016.hkbTransitionEffect castValue)
                {
                    instance.m_generatorChangedTransitionEffect = castValue;
                    return true;
                }
                return false;
            }
            case "m_checkAnimEndSlotNo":
            case "checkAnimEndSlotNo":
            {
                if (value is not int castValue) return false;
                instance.m_checkAnimEndSlotNo = castValue;
                return true;
            }
            case "m_replanningAI":
            case "replanningAI":
            {
                if (value is Havok.hk2016.CustomManualSelectorGenerator.ReplanningAI castValue)
                {
                    instance.m_replanningAI = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_replanningAI = (Havok.hk2016.CustomManualSelectorGenerator.ReplanningAI)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
