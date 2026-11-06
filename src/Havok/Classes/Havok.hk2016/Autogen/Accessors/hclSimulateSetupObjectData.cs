// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclSimulateSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclSimulateSetupObject>
{
    public hclSimulateSetupObjectData(Havok.hk2016.hclSimulateSetupObject instance) : base(instance) { }

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
            case "m_simClothSetupObject":
            case "simClothSetupObject":
            {
                value = instance.m_simClothSetupObject;
                return true;
            }
            case "m_simulateConfigs":
            case "simulateConfigs":
            {
                value = instance.m_simulateConfigs;
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
            case "m_simClothSetupObject":
            case "simClothSetupObject":
            {
                if (value is null)
                {
                    instance.m_simClothSetupObject = default;
                    return true;
                }
                if (value is Havok.hk2016.hclSimClothSetupObject castValue)
                {
                    instance.m_simClothSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_simulateConfigs":
            case "simulateConfigs":
            {
                if (value is not List<Havok.hk2016.hclSimulateSetupObject.Config> castValue) return false;
                instance.m_simulateConfigs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
