// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpVehicleInstanceWheelInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpVehicleInstance.WheelInfo>
{
    public hknpVehicleInstanceWheelInfoData(Havok.hk2018.hknpVehicleInstance.WheelInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_contactPoint":
            case "contactPoint":
            {
                value = instance.m_contactPoint;
                return true;
            }
            case "m_contactFriction":
            case "contactFriction":
            {
                value = instance.m_contactFriction;
                return true;
            }
            case "m_contactShapeKey":
            case "contactShapeKey":
            {
                value = instance.m_contactShapeKey;
                return true;
            }
            case "m_hardPointWs":
            case "hardPointWs":
            {
                value = instance.m_hardPointWs;
                return true;
            }
            case "m_rayEndPointWs":
            case "rayEndPointWs":
            {
                value = instance.m_rayEndPointWs;
                return true;
            }
            case "m_currentSuspensionLength":
            case "currentSuspensionLength":
            {
                value = instance.m_currentSuspensionLength;
                return true;
            }
            case "m_suspensionDirectionWs":
            case "suspensionDirectionWs":
            {
                value = instance.m_suspensionDirectionWs;
                return true;
            }
            case "m_spinAxisChassisSpace":
            case "spinAxisChassisSpace":
            {
                value = instance.m_spinAxisChassisSpace;
                return true;
            }
            case "m_spinAxisWs":
            case "spinAxisWs":
            {
                value = instance.m_spinAxisWs;
                return true;
            }
            case "m_steeringOrientationChassisSpace":
            case "steeringOrientationChassisSpace":
            {
                value = instance.m_steeringOrientationChassisSpace;
                return true;
            }
            case "m_spinVelocity":
            case "spinVelocity":
            {
                value = instance.m_spinVelocity;
                return true;
            }
            case "m_noSlipIdealSpinVelocity":
            case "noSlipIdealSpinVelocity":
            {
                value = instance.m_noSlipIdealSpinVelocity;
                return true;
            }
            case "m_spinAngle":
            case "spinAngle":
            {
                value = instance.m_spinAngle;
                return true;
            }
            case "m_skidEnergyDensity":
            case "skidEnergyDensity":
            {
                value = instance.m_skidEnergyDensity;
                return true;
            }
            case "m_sideForce":
            case "sideForce":
            {
                value = instance.m_sideForce;
                return true;
            }
            case "m_forwardSlipVelocity":
            case "forwardSlipVelocity":
            {
                value = instance.m_forwardSlipVelocity;
                return true;
            }
            case "m_sideSlipVelocity":
            case "sideSlipVelocity":
            {
                value = instance.m_sideSlipVelocity;
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
            case "m_contactPoint":
            case "contactPoint":
            {
                if (value is not Havok.hk2018.hkContactPoint castValue) return false;
                instance.m_contactPoint = castValue;
                return true;
            }
            case "m_contactFriction":
            case "contactFriction":
            {
                if (value is not float castValue) return false;
                instance.m_contactFriction = castValue;
                return true;
            }
            case "m_contactShapeKey":
            case "contactShapeKey":
            {
                if (value is not uint castValue) return false;
                instance.m_contactShapeKey = castValue;
                return true;
            }
            case "m_hardPointWs":
            case "hardPointWs":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_hardPointWs = castValue;
                return true;
            }
            case "m_rayEndPointWs":
            case "rayEndPointWs":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_rayEndPointWs = castValue;
                return true;
            }
            case "m_currentSuspensionLength":
            case "currentSuspensionLength":
            {
                if (value is not float castValue) return false;
                instance.m_currentSuspensionLength = castValue;
                return true;
            }
            case "m_suspensionDirectionWs":
            case "suspensionDirectionWs":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_suspensionDirectionWs = castValue;
                return true;
            }
            case "m_spinAxisChassisSpace":
            case "spinAxisChassisSpace":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_spinAxisChassisSpace = castValue;
                return true;
            }
            case "m_spinAxisWs":
            case "spinAxisWs":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_spinAxisWs = castValue;
                return true;
            }
            case "m_steeringOrientationChassisSpace":
            case "steeringOrientationChassisSpace":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_steeringOrientationChassisSpace = castValue;
                return true;
            }
            case "m_spinVelocity":
            case "spinVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_spinVelocity = castValue;
                return true;
            }
            case "m_noSlipIdealSpinVelocity":
            case "noSlipIdealSpinVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_noSlipIdealSpinVelocity = castValue;
                return true;
            }
            case "m_spinAngle":
            case "spinAngle":
            {
                if (value is not float castValue) return false;
                instance.m_spinAngle = castValue;
                return true;
            }
            case "m_skidEnergyDensity":
            case "skidEnergyDensity":
            {
                if (value is not float castValue) return false;
                instance.m_skidEnergyDensity = castValue;
                return true;
            }
            case "m_sideForce":
            case "sideForce":
            {
                if (value is not float castValue) return false;
                instance.m_sideForce = castValue;
                return true;
            }
            case "m_forwardSlipVelocity":
            case "forwardSlipVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_forwardSlipVelocity = castValue;
                return true;
            }
            case "m_sideSlipVelocity":
            case "sideSlipVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_sideSlipVelocity = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
