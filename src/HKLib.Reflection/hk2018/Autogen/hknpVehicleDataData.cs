// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpVehicleDataData : HavokData<hknpVehicleData> 
{
    public hknpVehicleDataData(HavokType type, hknpVehicleData instance) : base(type, instance) {}

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
            case "m_gravity":
            case "gravity":
            {
                if (instance.m_gravity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numWheels":
            case "numWheels":
            {
                if (instance.m_numWheels is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_chassisOrientation":
            case "chassisOrientation":
            {
                if (instance.m_chassisOrientation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_torqueRollFactor":
            case "torqueRollFactor":
            {
                if (instance.m_torqueRollFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_torquePitchFactor":
            case "torquePitchFactor":
            {
                if (instance.m_torquePitchFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_torqueYawFactor":
            case "torqueYawFactor":
            {
                if (instance.m_torqueYawFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_extraTorqueFactor":
            case "extraTorqueFactor":
            {
                if (instance.m_extraTorqueFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxVelocityForPositionalFriction":
            case "maxVelocityForPositionalFriction":
            {
                if (instance.m_maxVelocityForPositionalFriction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_chassisUnitInertiaYaw":
            case "chassisUnitInertiaYaw":
            {
                if (instance.m_chassisUnitInertiaYaw is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_chassisUnitInertiaRoll":
            case "chassisUnitInertiaRoll":
            {
                if (instance.m_chassisUnitInertiaRoll is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_chassisUnitInertiaPitch":
            case "chassisUnitInertiaPitch":
            {
                if (instance.m_chassisUnitInertiaPitch is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_frictionEqualizer":
            case "frictionEqualizer":
            {
                if (instance.m_frictionEqualizer is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_normalClippingAngleCos":
            case "normalClippingAngleCos":
            {
                if (instance.m_normalClippingAngleCos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxFrictionSolverMassRatio":
            case "maxFrictionSolverMassRatio":
            {
                if (instance.m_maxFrictionSolverMassRatio is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wheelParams":
            case "wheelParams":
            {
                if (instance.m_wheelParams is not TGet castValue) return false;
                value = castValue;
                return true;
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
                if (value is not List<hknpVehicleData.WheelComponentParams> castValue) return false;
                instance.m_wheelParams = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
