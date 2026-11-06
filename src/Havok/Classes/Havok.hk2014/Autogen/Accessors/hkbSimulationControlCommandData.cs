// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbSimulationControlCommandData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbSimulationControlCommand>
{
    public hkbSimulationControlCommandData(Havok.hk2014.hkbSimulationControlCommand instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_command":
            case "command":
            {
                value = instance.m_command;
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
            case "m_command":
            case "command":
            {
                if (value is Havok.hk2014.hkbSimulationControlCommand.SimulationControlCommand castValue)
                {
                    instance.m_command = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_command = (Havok.hk2014.hkbSimulationControlCommand.SimulationControlCommand)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
