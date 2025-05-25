// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbBehaviorGraphDataData : HavokData<HKLib.hk2018.hkbBehaviorGraphData> 
{
    public hkbBehaviorGraphDataData(HavokType type, HKLib.hk2018.hkbBehaviorGraphData instance) : base(type, instance) {}

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
            case "m_attributeDefaults":
            case "attributeDefaults":
            {
                if (instance.m_attributeDefaults is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_variableInfos":
            case "variableInfos":
            {
                if (instance.m_variableInfos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterPropertyInfos":
            case "characterPropertyInfos":
            {
                if (instance.m_characterPropertyInfos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_eventInfos":
            case "eventInfos":
            {
                if (instance.m_eventInfos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_variableBounds":
            case "variableBounds":
            {
                if (instance.m_variableBounds is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_variableInitialValues":
            case "variableInitialValues":
            {
                if (instance.m_variableInitialValues is null)
                {
                    return true;
                }
                if (instance.m_variableInitialValues is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_stringData":
            case "stringData":
            {
                if (instance.m_stringData is null)
                {
                    return true;
                }
                if (instance.m_stringData is TGet castValue)
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
            case "m_attributeDefaults":
            case "attributeDefaults":
            {
                if (value is not List<float> castValue) return false;
                instance.m_attributeDefaults = castValue;
                return true;
            }
            case "m_variableInfos":
            case "variableInfos":
            {
                if (value is not List<hkbVariableInfo> castValue) return false;
                instance.m_variableInfos = castValue;
                return true;
            }
            case "m_characterPropertyInfos":
            case "characterPropertyInfos":
            {
                if (value is not List<hkbVariableInfo> castValue) return false;
                instance.m_characterPropertyInfos = castValue;
                return true;
            }
            case "m_eventInfos":
            case "eventInfos":
            {
                if (value is not List<hkbEventInfo> castValue) return false;
                instance.m_eventInfos = castValue;
                return true;
            }
            case "m_variableBounds":
            case "variableBounds":
            {
                if (value is not List<hkbVariableBounds> castValue) return false;
                instance.m_variableBounds = castValue;
                return true;
            }
            case "m_variableInitialValues":
            case "variableInitialValues":
            {
                if (value is null)
                {
                    instance.m_variableInitialValues = default;
                    return true;
                }
                if (value is hkbVariableValueSet castValue)
                {
                    instance.m_variableInitialValues = castValue;
                    return true;
                }
                return false;
            }
            case "m_stringData":
            case "stringData":
            {
                if (value is null)
                {
                    instance.m_stringData = default;
                    return true;
                }
                if (value is hkbBehaviorGraphStringData castValue)
                {
                    instance.m_stringData = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
