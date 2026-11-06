// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclSimulateOperatorInstanceDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclSimulateOperatorInstanceData>
{
    public hclSimulateOperatorInstanceDataData(Havok.hk2018.hclSimulateOperatorInstanceData instance) : base(instance) { }

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
            case "m_stateId":
            case "stateId":
            {
                value = instance.m_stateId;
                return true;
            }
            case "m_configIndex":
            case "configIndex":
            {
                value = instance.m_configIndex;
                return true;
            }
            case "m_overriddenSubSteps":
            case "overriddenSubSteps":
            {
                value = instance.m_overriddenSubSteps;
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
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_stateId":
            case "stateId":
            {
                if (value is not uint castValue) return false;
                instance.m_stateId = castValue;
                return true;
            }
            case "m_configIndex":
            case "configIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_configIndex = castValue;
                return true;
            }
            case "m_overriddenSubSteps":
            case "overriddenSubSteps":
            {
                if (value is not uint castValue) return false;
                instance.m_overriddenSubSteps = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
