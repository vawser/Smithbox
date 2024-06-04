// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbSimulationControlCommandData : HavokData<hkbSimulationControlCommand> 
{
    public hkbSimulationControlCommandData(HavokType type, hkbSimulationControlCommand instance) : base(type, instance) {}

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
            case "m_command":
            case "command":
            {
                if (instance.m_command is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_command is TGet byteValue)
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
            case "m_command":
            case "command":
            {
                if (value is hkbSimulationControlCommand.SimulationControlCommand castValue)
                {
                    instance.m_command = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_command = (hkbSimulationControlCommand.SimulationControlCommand)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
