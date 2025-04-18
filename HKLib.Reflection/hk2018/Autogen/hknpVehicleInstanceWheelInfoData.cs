// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpVehicleInstanceWheelInfoData : HavokData<hknpVehicleInstance.WheelInfo> 
{
    public hknpVehicleInstanceWheelInfoData(HavokType type, hknpVehicleInstance.WheelInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_contactPoint":
            case "contactPoint":
            {
                if (instance.m_contactPoint is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_contactFriction":
            case "contactFriction":
            {
                if (instance.m_contactFriction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_contactShapeKey":
            case "contactShapeKey":
            {
                if (instance.m_contactShapeKey is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hardPointWs":
            case "hardPointWs":
            {
                if (instance.m_hardPointWs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rayEndPointWs":
            case "rayEndPointWs":
            {
                if (instance.m_rayEndPointWs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentSuspensionLength":
            case "currentSuspensionLength":
            {
                if (instance.m_currentSuspensionLength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_suspensionDirectionWs":
            case "suspensionDirectionWs":
            {
                if (instance.m_suspensionDirectionWs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_spinAxisChassisSpace":
            case "spinAxisChassisSpace":
            {
                if (instance.m_spinAxisChassisSpace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_spinAxisWs":
            case "spinAxisWs":
            {
                if (instance.m_spinAxisWs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_steeringOrientationChassisSpace":
            case "steeringOrientationChassisSpace":
            {
                if (instance.m_steeringOrientationChassisSpace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_spinVelocity":
            case "spinVelocity":
            {
                if (instance.m_spinVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_noSlipIdealSpinVelocity":
            case "noSlipIdealSpinVelocity":
            {
                if (instance.m_noSlipIdealSpinVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_spinAngle":
            case "spinAngle":
            {
                if (instance.m_spinAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_skidEnergyDensity":
            case "skidEnergyDensity":
            {
                if (instance.m_skidEnergyDensity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sideForce":
            case "sideForce":
            {
                if (instance.m_sideForce is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_forwardSlipVelocity":
            case "forwardSlipVelocity":
            {
                if (instance.m_forwardSlipVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sideSlipVelocity":
            case "sideSlipVelocity":
            {
                if (instance.m_sideSlipVelocity is not TGet castValue) return false;
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
            case "m_contactPoint":
            case "contactPoint":
            {
                if (value is not hkContactPoint castValue) return false;
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
