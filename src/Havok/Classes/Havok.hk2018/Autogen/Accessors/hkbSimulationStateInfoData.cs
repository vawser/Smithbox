// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbSimulationStateInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbSimulationStateInfo>
{
    public hkbSimulationStateInfoData(Havok.hk2018.hkbSimulationStateInfo instance) : base(instance) { }

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
            case "m_simulationState":
            case "simulationState":
            {
                value = instance.m_simulationState;
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
            case "m_simulationState":
            case "simulationState":
            {
                if (value is Havok.hk2018.hkbWorldEnums.SimulationState castValue)
                {
                    instance.m_simulationState = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_simulationState = (Havok.hk2018.hkbWorldEnums.SimulationState)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
