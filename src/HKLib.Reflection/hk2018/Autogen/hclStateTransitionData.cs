// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStateTransitionData : HavokData<hclStateTransition> 
{
    public hclStateTransitionData(HavokType type, hclStateTransition instance) : base(type, instance) {}

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
            case "m_stateIds":
            case "stateIds":
            {
                if (instance.m_stateIds is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stateTransitionData":
            case "stateTransitionData":
            {
                if (instance.m_stateTransitionData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simClothTransitionConstraints":
            case "simClothTransitionConstraints":
            {
                if (instance.m_simClothTransitionConstraints is not TGet castValue) return false;
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
            case "m_stateIds":
            case "stateIds":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_stateIds = castValue;
                return true;
            }
            case "m_stateTransitionData":
            case "stateTransitionData":
            {
                if (value is not List<hclStateTransition.StateTransitionData> castValue) return false;
                instance.m_stateTransitionData = castValue;
                return true;
            }
            case "m_simClothTransitionConstraints":
            case "simClothTransitionConstraints":
            {
                if (value is not List<List<hkHandle<uint>>> castValue) return false;
                instance.m_simClothTransitionConstraints = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
