// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbBehaviorGraphInternalStateData : HavokData<hkbBehaviorGraphInternalState> 
{
    public hkbBehaviorGraphInternalStateData(HavokType type, hkbBehaviorGraphInternalState instance) : base(type, instance) {}

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
            case "m_nodeInternalStateInfos":
            case "nodeInternalStateInfos":
            {
                if (instance.m_nodeInternalStateInfos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_variableValueSet":
            case "variableValueSet":
            {
                if (instance.m_variableValueSet is null)
                {
                    return true;
                }
                if (instance.m_variableValueSet is TGet castValue)
                {
                    value = castValue;
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
            case "m_nodeInternalStateInfos":
            case "nodeInternalStateInfos":
            {
                if (value is not List<hkbNodeInternalStateInfo?> castValue) return false;
                instance.m_nodeInternalStateInfos = castValue;
                return true;
            }
            case "m_variableValueSet":
            case "variableValueSet":
            {
                if (value is null)
                {
                    instance.m_variableValueSet = default;
                    return true;
                }
                if (value is hkbVariableValueSet castValue)
                {
                    instance.m_variableValueSet = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
