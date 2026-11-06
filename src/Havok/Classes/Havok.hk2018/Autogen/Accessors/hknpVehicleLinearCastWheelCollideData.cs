// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpVehicleLinearCastWheelCollideData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpVehicleLinearCastWheelCollide>
{
    public hknpVehicleLinearCastWheelCollideData(Havok.hk2018.hknpVehicleLinearCastWheelCollide instance) : base(instance) { }

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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
                if (value is not List<Havok.hk2018.hknpVehicleLinearCastWheelCollide.WheelState> castValue) return false;
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
                if (value is not Havok.hk2018.hknpBodyId castValue) return false;
                instance.m_chassisBody = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
