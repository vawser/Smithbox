// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStateTransitionSetupObjectData : HavokData<hclStateTransitionSetupObject> 
{
    public hclStateTransitionSetupObjectData(HavokType type, hclStateTransitionSetupObject instance) : base(type, instance) {}

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
            case "m_useTransitionConstraints":
            case "useTransitionConstraints":
            {
                if (instance.m_useTransitionConstraints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useDynamicBlendTransitions":
            case "useDynamicBlendTransitions":
            {
                if (instance.m_useDynamicBlendTransitions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stateSetupObjects":
            case "stateSetupObjects":
            {
                if (instance.m_stateSetupObjects is not TGet castValue) return false;
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
            case "m_useTransitionConstraints":
            case "useTransitionConstraints":
            {
                if (value is not bool castValue) return false;
                instance.m_useTransitionConstraints = castValue;
                return true;
            }
            case "m_useDynamicBlendTransitions":
            case "useDynamicBlendTransitions":
            {
                if (value is not bool castValue) return false;
                instance.m_useDynamicBlendTransitions = castValue;
                return true;
            }
            case "m_stateSetupObjects":
            case "stateSetupObjects":
            {
                if (value is not List<hclClothStateSetupObject?> castValue) return false;
                instance.m_stateSetupObjects = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
