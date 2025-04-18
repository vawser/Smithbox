// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclClothInstanceData : HavokData<hclClothInstance> 
{
    public hclClothInstanceData(HavokType type, hclClothInstance instance) : base(type, instance) {}

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
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stateTransitionIndex":
            case "stateTransitionIndex":
            {
                if (instance.m_stateTransitionIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stateTransitionStateIndex":
            case "stateTransitionStateIndex":
            {
                if (instance.m_stateTransitionStateIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stateTiming":
            case "stateTiming":
            {
                if (instance.m_stateTiming is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_update":
            case "update":
            {
                if (instance.m_update is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_multithreaded":
            case "multithreaded":
            {
                if (instance.m_multithreaded is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentState":
            case "currentState":
            {
                if (instance.m_currentState is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_viewersEnabled":
            case "viewersEnabled":
            {
                if (instance.m_viewersEnabled is not TGet castValue) return false;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_stateTransitionIndex":
            case "stateTransitionIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_stateTransitionIndex = castValue;
                return true;
            }
            case "m_stateTransitionStateIndex":
            case "stateTransitionStateIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_stateTransitionStateIndex = castValue;
                return true;
            }
            case "m_stateTiming":
            case "stateTiming":
            {
                if (value is not float castValue) return false;
                instance.m_stateTiming = castValue;
                return true;
            }
            case "m_update":
            case "update":
            {
                if (value is not bool castValue) return false;
                instance.m_update = castValue;
                return true;
            }
            case "m_multithreaded":
            case "multithreaded":
            {
                if (value is not bool castValue) return false;
                instance.m_multithreaded = castValue;
                return true;
            }
            case "m_currentState":
            case "currentState":
            {
                if (value is not uint castValue) return false;
                instance.m_currentState = castValue;
                return true;
            }
            case "m_viewersEnabled":
            case "viewersEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_viewersEnabled = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
