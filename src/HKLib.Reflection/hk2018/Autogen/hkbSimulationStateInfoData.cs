// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbSimulationStateInfoData : HavokData<hkbSimulationStateInfo> 
{
    public hkbSimulationStateInfoData(HavokType type, hkbSimulationStateInfo instance) : base(type, instance) {}

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
            case "m_simulationState":
            case "simulationState":
            {
                if (instance.m_simulationState is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_simulationState is TGet byteValue)
                {
                    value = byteValue;
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
            case "m_simulationState":
            case "simulationState":
            {
                if (value is hkbWorldEnums.SimulationState castValue)
                {
                    instance.m_simulationState = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_simulationState = (hkbWorldEnums.SimulationState)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
