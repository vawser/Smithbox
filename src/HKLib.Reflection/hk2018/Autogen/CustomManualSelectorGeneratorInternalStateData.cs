// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class CustomManualSelectorGeneratorInternalStateData : HavokData<CustomManualSelectorGeneratorInternalState> 
{
    public CustomManualSelectorGeneratorInternalStateData(HavokType type, CustomManualSelectorGeneratorInternalState instance) : base(type, instance) {}

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
            case "m_currentGeneratorIndex":
            case "currentGeneratorIndex":
            {
                if (instance.m_currentGeneratorIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_generatorIndexAtActivate":
            case "generatorIndexAtActivate":
            {
                if (instance.m_generatorIndexAtActivate is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_activeTransitions":
            case "activeTransitions":
            {
                if (instance.m_activeTransitions is not TGet castValue) return false;
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
            case "m_currentGeneratorIndex":
            case "currentGeneratorIndex":
            {
                if (value is not short castValue) return false;
                instance.m_currentGeneratorIndex = castValue;
                return true;
            }
            case "m_generatorIndexAtActivate":
            case "generatorIndexAtActivate":
            {
                if (value is not short castValue) return false;
                instance.m_generatorIndexAtActivate = castValue;
                return true;
            }
            case "m_activeTransitions":
            case "activeTransitions":
            {
                if (value is not List<hkbStateMachine.ActiveTransitionInfo> castValue) return false;
                instance.m_activeTransitions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
