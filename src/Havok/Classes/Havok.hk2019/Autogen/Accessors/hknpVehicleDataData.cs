// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpVehicleDataData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpVehicleData>
{
    public hknpVehicleDataData(Havok.hk2019.hknpVehicleData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_gravity":
            case "gravity":
            {
                value = instance.m_gravity;
                return true;
            }
            case "m_numWheels":
            case "numWheels":
            {
                value = instance.m_numWheels;
                return true;
            }
            case "m_chassisOrientation":
            case "chassisOrientation":
            {
                value = instance.m_chassisOrientation;
                return true;
            }
            case "m_torqueRollFactor":
            case "torqueRollFactor":
            {
                value = instance.m_torqueRollFactor;
                return true;
            }
            case "m_torquePitchFactor":
            case "torquePitchFactor":
            {
                value = instance.m_torquePitchFactor;
                return true;
            }
            case "m_torqueYawFactor":
            case "torqueYawFactor":
            {
                value = instance.m_torqueYawFactor;
                return true;
            }
            case "m_extraTorqueFactor":
            case "extraTorqueFactor":
            {
                value = instance.m_extraTorqueFactor;
                return true;
            }
            case "m_maxVelocityForPositionalFriction":
            case "maxVelocityForPositionalFriction":
            {
                value = instance.m_maxVelocityForPositionalFriction;
                return true;
            }
            case "m_chassisUnitInertiaYaw":
            case "chassisUnitInertiaYaw":
            {
                value = instance.m_chassisUnitInertiaYaw;
                return true;
            }
            case "m_chassisUnitInertiaRoll":
            case "chassisUnitInertiaRoll":
            {
                value = instance.m_chassisUnitInertiaRoll;
                return true;
            }
            case "m_chassisUnitInertiaPitch":
            case "chassisUnitInertiaPitch":
            {
                value = instance.m_chassisUnitInertiaPitch;
                return true;
            }
            case "m_frictionEqualizer":
            case "frictionEqualizer":
            {
                value = instance.m_frictionEqualizer;
                return true;
            }
            case "m_normalClippingAngleCos":
            case "normalClippingAngleCos":
            {
                value = instance.m_normalClippingAngleCos;
                return true;
            }
            case "m_maxFrictionSolverMassRatio":
            case "maxFrictionSolverMassRatio":
            {
                value = instance.m_maxFrictionSolverMassRatio;
                return true;
            }
            case "m_wheelParams":
            case "wheelParams":
            {
                value = instance.m_wheelParams;
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
            case "m_gravity":
            case "gravity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_gravity = castValue;
                return true;
            }
            case "m_numWheels":
            case "numWheels":
            {
                if (value is not sbyte castValue) return false;
                instance.m_numWheels = castValue;
                return true;
            }
            case "m_chassisOrientation":
            case "chassisOrientation":
            {
                if (value is not Matrix3x3 castValue) return false;
                instance.m_chassisOrientation = castValue;
                return true;
            }
            case "m_torqueRollFactor":
            case "torqueRollFactor":
            {
                if (value is not float castValue) return false;
                instance.m_torqueRollFactor = castValue;
                return true;
            }
            case "m_torquePitchFactor":
            case "torquePitchFactor":
            {
                if (value is not float castValue) return false;
                instance.m_torquePitchFactor = castValue;
                return true;
            }
            case "m_torqueYawFactor":
            case "torqueYawFactor":
            {
                if (value is not float castValue) return false;
                instance.m_torqueYawFactor = castValue;
                return true;
            }
            case "m_extraTorqueFactor":
            case "extraTorqueFactor":
            {
                if (value is not float castValue) return false;
                instance.m_extraTorqueFactor = castValue;
                return true;
            }
            case "m_maxVelocityForPositionalFriction":
            case "maxVelocityForPositionalFriction":
            {
                if (value is not float castValue) return false;
                instance.m_maxVelocityForPositionalFriction = castValue;
                return true;
            }
            case "m_chassisUnitInertiaYaw":
            case "chassisUnitInertiaYaw":
            {
                if (value is not float castValue) return false;
                instance.m_chassisUnitInertiaYaw = castValue;
                return true;
            }
            case "m_chassisUnitInertiaRoll":
            case "chassisUnitInertiaRoll":
            {
                if (value is not float castValue) return false;
                instance.m_chassisUnitInertiaRoll = castValue;
                return true;
            }
            case "m_chassisUnitInertiaPitch":
            case "chassisUnitInertiaPitch":
            {
                if (value is not float castValue) return false;
                instance.m_chassisUnitInertiaPitch = castValue;
                return true;
            }
            case "m_frictionEqualizer":
            case "frictionEqualizer":
            {
                if (value is not float castValue) return false;
                instance.m_frictionEqualizer = castValue;
                return true;
            }
            case "m_normalClippingAngleCos":
            case "normalClippingAngleCos":
            {
                if (value is not float castValue) return false;
                instance.m_normalClippingAngleCos = castValue;
                return true;
            }
            case "m_maxFrictionSolverMassRatio":
            case "maxFrictionSolverMassRatio":
            {
                if (value is not float castValue) return false;
                instance.m_maxFrictionSolverMassRatio = castValue;
                return true;
            }
            case "m_wheelParams":
            case "wheelParams":
            {
                if (value is not List<Havok.hk2019.hknpVehicleData.WheelComponentParams> castValue) return false;
                instance.m_wheelParams = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
