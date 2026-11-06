// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclStateTransitionData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclStateTransition>
{
    public hclStateTransitionData(Havok.hk2016.hclStateTransition instance) : base(instance) { }

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
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_stateIds":
            case "stateIds":
            {
                value = instance.m_stateIds;
                return true;
            }
            case "m_stateTransitionData":
            case "stateTransitionData":
            {
                value = instance.m_stateTransitionData;
                return true;
            }
            case "m_simClothTransitionConstraints":
            case "simClothTransitionConstraints":
            {
                value = instance.m_simClothTransitionConstraints;
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
                if (value is not List<Havok.hk2016.hclStateTransition.StateTransitionData> castValue) return false;
                instance.m_stateTransitionData = castValue;
                return true;
            }
            case "m_simClothTransitionConstraints":
            case "simClothTransitionConstraints":
            {
                if (value is not List<List<int>> castValue) return false;
                instance.m_simClothTransitionConstraints = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
