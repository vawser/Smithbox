// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpVehicleDefaultTransmissionData : HavokData<hknpVehicleDefaultTransmission> 
{
    public hknpVehicleDefaultTransmissionData(HavokType type, hknpVehicleDefaultTransmission instance) : base(type, instance) {}

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
            case "m_downshiftRPM":
            case "downshiftRPM":
            {
                if (instance.m_downshiftRPM is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_upshiftRPM":
            case "upshiftRPM":
            {
                if (instance.m_upshiftRPM is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_primaryTransmissionRatio":
            case "primaryTransmissionRatio":
            {
                if (instance.m_primaryTransmissionRatio is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clutchDelayTime":
            case "clutchDelayTime":
            {
                if (instance.m_clutchDelayTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_reverseGearRatio":
            case "reverseGearRatio":
            {
                if (instance.m_reverseGearRatio is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gearsRatio":
            case "gearsRatio":
            {
                if (instance.m_gearsRatio is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wheelsTorqueRatio":
            case "wheelsTorqueRatio":
            {
                if (instance.m_wheelsTorqueRatio is not TGet castValue) return false;
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
