// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpVehicleLinearCastWheelCollideData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpVehicleLinearCastWheelCollide>
{
    public hknpVehicleLinearCastWheelCollideData(Havok.hk2014.hknpVehicleLinearCastWheelCollide instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_alreadyUsed":
            case "alreadyUsed":
            {
                value = instance.m_alreadyUsed;
                return true;
            }
            case "m_wheelStates":
            case "wheelStates":
            {
                value = instance.m_wheelStates;
                return true;
            }
            case "m_maxExtraPenetration":
            case "maxExtraPenetration":
            {
                value = instance.m_maxExtraPenetration;
                return true;
            }
            case "m_startPointTolerance":
            case "startPointTolerance":
            {
                value = instance.m_startPointTolerance;
                return true;
            }
            case "m_chassisBody":
            case "chassisBody":
            {
                value = instance.m_chassisBody;
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
            case "m_alreadyUsed":
            case "alreadyUsed":
            {
                if (value is not bool castValue) return false;
                instance.m_alreadyUsed = castValue;
                return true;
            }
            case "m_wheelStates":
            case "wheelStates":
            {
                if (value is not List<Havok.hk2014.hknpVehicleLinearCastWheelCollide.WheelState> castValue) return false;
                instance.m_wheelStates = castValue;
                return true;
            }
            case "m_maxExtraPenetration":
            case "maxExtraPenetration":
            {
                if (value is not float castValue) return false;
                instance.m_maxExtraPenetration = castValue;
                return true;
            }
            case "m_startPointTolerance":
            case "startPointTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_startPointTolerance = castValue;
                return true;
            }
            case "m_chassisBody":
            case "chassisBody":
            {
                if (value is not uint castValue) return false;
                instance.m_chassisBody = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
