// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbSimulationStateInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbSimulationStateInfo>
{
    public hkbSimulationStateInfoData(Havok.hk2014.hkbSimulationStateInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_simulationState":
            case "simulationState":
            {
                if (value is Havok.hk2014.hkbWorldEnums.SimulationState castValue)
                {
                    instance.m_simulationState = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_simulationState = (Havok.hk2014.hkbWorldEnums.SimulationState)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
