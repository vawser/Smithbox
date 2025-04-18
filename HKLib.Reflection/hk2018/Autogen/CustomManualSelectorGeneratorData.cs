// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class CustomManualSelectorGeneratorData : HavokData<CustomManualSelectorGenerator> 
{
    public CustomManualSelectorGeneratorData(HavokType type, CustomManualSelectorGenerator instance) : base(type, instance) {}

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
            case "m_offsetType":
            case "offsetType":
            {
                if (instance.m_offsetType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_offsetType is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_animId":
            case "animId":
            {
                if (instance.m_animId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_animeEndEventType":
            case "animeEndEventType":
            {
                if (instance.m_animeEndEventType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_animeEndEventType is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_enableScript":
            case "enableScript":
            {
                if (instance.m_enableScript is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableTae":
            case "enableTae":
            {
                if (instance.m_enableTae is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_changeTypeOfSelectedIndexAfterActivate":
            case "changeTypeOfSelectedIndexAfterActivate":
            {
                if (instance.m_changeTypeOfSelectedIndexAfterActivate is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_changeTypeOfSelectedIndexAfterActivate is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
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
            case "m_checkAnimEndSlotNo":
            case "checkAnimEndSlotNo":
            {
                if (instance.m_checkAnimEndSlotNo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_replanningAI":
            case "replanningAI":
            {
                if (instance.m_replanningAI is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_replanningAI is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_rideSync":
            case "rideSync":
            {
                if (instance.m_rideSync is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_rideSync is TGet byteValue)
                {
                    value = byteValue;
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
            case "m_offsetType":
            case "offsetType":
            {
                if (value is CustomManualSelectorGenerator.OffsetType castValue)
                {
                    instance.m_offsetType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_offsetType = (CustomManualSelectorGenerator.OffsetType)intValue;
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
                if (value is CustomManualSelectorGenerator.AnimeEndEventType castValue)
                {
                    instance.m_animeEndEventType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_animeEndEventType = (CustomManualSelectorGenerator.AnimeEndEventType)intValue;
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
                if (value is CustomManualSelectorGenerator.ChangeTypeOfSelectedIndexAfterActivate castValue)
                {
                    instance.m_changeTypeOfSelectedIndexAfterActivate = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_changeTypeOfSelectedIndexAfterActivate = (CustomManualSelectorGenerator.ChangeTypeOfSelectedIndexAfterActivate)byteValue;
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
                if (value is hkbTransitionEffect castValue)
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
                if (value is CustomManualSelectorGenerator.ReplanningAI castValue)
                {
                    instance.m_replanningAI = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_replanningAI = (CustomManualSelectorGenerator.ReplanningAI)byteValue;
                    return true;
                }
                return false;
            }
            case "m_rideSync":
            case "rideSync":
            {
                if (value is CustomManualSelectorGenerator.RideSync castValue)
                {
                    instance.m_rideSync = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_rideSync = (CustomManualSelectorGenerator.RideSync)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
