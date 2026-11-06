// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpVehicleDefaultTransmissionData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpVehicleDefaultTransmission>
{
    public hknpVehicleDefaultTransmissionData(Havok.hk2019.hknpVehicleDefaultTransmission instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_downshiftRPM":
            case "downshiftRPM":
            {
                value = instance.m_downshiftRPM;
                return true;
            }
            case "m_upshiftRPM":
            case "upshiftRPM":
            {
                value = instance.m_upshiftRPM;
                return true;
            }
            case "m_primaryTransmissionRatio":
            case "primaryTransmissionRatio":
            {
                value = instance.m_primaryTransmissionRatio;
                return true;
            }
            case "m_clutchDelayTime":
            case "clutchDelayTime":
            {
                value = instance.m_clutchDelayTime;
                return true;
            }
            case "m_reverseGearRatio":
            case "reverseGearRatio":
            {
                value = instance.m_reverseGearRatio;
                return true;
            }
            case "m_gearsRatio":
            case "gearsRatio":
            {
                value = instance.m_gearsRatio;
                return true;
            }
            case "m_wheelsTorqueRatio":
            case "wheelsTorqueRatio":
            {
                value = instance.m_wheelsTorqueRatio;
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
            case "m_downshiftRPM":
            case "downshiftRPM":
            {
                if (value is not float castValue) return false;
                instance.m_downshiftRPM = castValue;
                return true;
            }
            case "m_upshiftRPM":
            case "upshiftRPM":
            {
                if (value is not float castValue) return false;
                instance.m_upshiftRPM = castValue;
                return true;
            }
            case "m_primaryTransmissionRatio":
            case "primaryTransmissionRatio":
            {
                if (value is not float castValue) return false;
                instance.m_primaryTransmissionRatio = castValue;
                return true;
            }
            case "m_clutchDelayTime":
            case "clutchDelayTime":
            {
                if (value is not float castValue) return false;
                instance.m_clutchDelayTime = castValue;
                return true;
            }
            case "m_reverseGearRatio":
            case "reverseGearRatio":
            {
                if (value is not float castValue) return false;
                instance.m_reverseGearRatio = castValue;
                return true;
            }
            case "m_gearsRatio":
            case "gearsRatio":
            {
                if (value is not List<float> castValue) return false;
                instance.m_gearsRatio = castValue;
                return true;
            }
            case "m_wheelsTorqueRatio":
            case "wheelsTorqueRatio":
            {
                if (value is not List<float> castValue) return false;
                instance.m_wheelsTorqueRatio = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
