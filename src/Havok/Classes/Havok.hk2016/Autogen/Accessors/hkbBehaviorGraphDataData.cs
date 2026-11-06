// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbBehaviorGraphDataData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbBehaviorGraphData>
{
    public hkbBehaviorGraphDataData(Havok.hk2016.hkbBehaviorGraphData instance) : base(instance) { }

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
            case "m_attributeDefaults":
            case "attributeDefaults":
            {
                value = instance.m_attributeDefaults;
                return true;
            }
            case "m_variableInfos":
            case "variableInfos":
            {
                value = instance.m_variableInfos;
                return true;
            }
            case "m_characterPropertyInfos":
            case "characterPropertyInfos":
            {
                value = instance.m_characterPropertyInfos;
                return true;
            }
            case "m_eventInfos":
            case "eventInfos":
            {
                value = instance.m_eventInfos;
                return true;
            }
            case "m_variableBounds":
            case "variableBounds":
            {
                value = instance.m_variableBounds;
                return true;
            }
            case "m_variableInitialValues":
            case "variableInitialValues":
            {
                value = instance.m_variableInitialValues;
                return true;
            }
            case "m_stringData":
            case "stringData":
            {
                value = instance.m_stringData;
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
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
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
                if (value is not List<Havok.hk2016.hkbVariableInfo> castValue) return false;
                instance.m_variableInfos = castValue;
                return true;
            }
            case "m_characterPropertyInfos":
            case "characterPropertyInfos":
            {
                if (value is not List<Havok.hk2016.hkbVariableInfo> castValue) return false;
                instance.m_characterPropertyInfos = castValue;
                return true;
            }
            case "m_eventInfos":
            case "eventInfos":
            {
                if (value is not List<Havok.hk2016.hkbEventInfo> castValue) return false;
                instance.m_eventInfos = castValue;
                return true;
            }
            case "m_variableBounds":
            case "variableBounds":
            {
                if (value is not List<Havok.hk2016.hkbVariableBounds> castValue) return false;
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
                if (value is Havok.hk2016.hkbVariableValueSet castValue)
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
                if (value is Havok.hk2016.hkbBehaviorGraphStringData castValue)
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
