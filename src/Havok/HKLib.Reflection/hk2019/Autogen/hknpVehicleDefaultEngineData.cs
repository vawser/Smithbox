// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpVehicleDefaultEngineData : HavokData<HKLib.hk2019.hknpVehicleDefaultEngine> 
{
    public hknpVehicleDefaultEngineData(HavokType type, HKLib.hk2019.hknpVehicleDefaultEngine instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_minRPM":
            case "minRPM":
            {
                if (instance.m_minRPM is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_optRPM":
            case "optRPM":
            {
                if (instance.m_optRPM is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxRPM":
            case "maxRPM":
            {
                if (instance.m_maxRPM is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxTorque":
            case "maxTorque":
            {
                if (instance.m_maxTorque is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_torqueFactorAtMinRPM":
            case "torqueFactorAtMinRPM":
            {
                if (instance.m_torqueFactorAtMinRPM is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_torqueFactorAtMaxRPM":
            case "torqueFactorAtMaxRPM":
            {
                if (instance.m_torqueFactorAtMaxRPM is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_resistanceFactorAtMinRPM":
            case "resistanceFactorAtMinRPM":
            {
                if (instance.m_resistanceFactorAtMinRPM is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_resistanceFactorAtOptRPM":
            case "resistanceFactorAtOptRPM":
            {
                if (instance.m_resistanceFactorAtOptRPM is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_resistanceFactorAtMaxRPM":
            case "resistanceFactorAtMaxRPM":
            {
                if (instance.m_resistanceFactorAtMaxRPM is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clutchSlipRPM":
            case "clutchSlipRPM":
            {
                if (instance.m_clutchSlipRPM is not TGet castValue) return false;
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
            case "m_minRPM":
            case "minRPM":
            {
                if (value is not float castValue) return false;
                instance.m_minRPM = castValue;
                return true;
            }
            case "m_optRPM":
            case "optRPM":
            {
                if (value is not float castValue) return false;
                instance.m_optRPM = castValue;
                return true;
            }
            case "m_maxRPM":
            case "maxRPM":
            {
                if (value is not float castValue) return false;
                instance.m_maxRPM = castValue;
                return true;
            }
            case "m_maxTorque":
            case "maxTorque":
            {
                if (value is not float castValue) return false;
                instance.m_maxTorque = castValue;
                return true;
            }
            case "m_torqueFactorAtMinRPM":
            case "torqueFactorAtMinRPM":
            {
                if (value is not float castValue) return false;
                instance.m_torqueFactorAtMinRPM = castValue;
                return true;
            }
            case "m_torqueFactorAtMaxRPM":
            case "torqueFactorAtMaxRPM":
            {
                if (value is not float castValue) return false;
                instance.m_torqueFactorAtMaxRPM = castValue;
                return true;
            }
            case "m_resistanceFactorAtMinRPM":
            case "resistanceFactorAtMinRPM":
            {
                if (value is not float castValue) return false;
                instance.m_resistanceFactorAtMinRPM = castValue;
                return true;
            }
            case "m_resistanceFactorAtOptRPM":
            case "resistanceFactorAtOptRPM":
            {
                if (value is not float castValue) return false;
                instance.m_resistanceFactorAtOptRPM = castValue;
                return true;
            }
            case "m_resistanceFactorAtMaxRPM":
            case "resistanceFactorAtMaxRPM":
            {
                if (value is not float castValue) return false;
                instance.m_resistanceFactorAtMaxRPM = castValue;
                return true;
            }
            case "m_clutchSlipRPM":
            case "clutchSlipRPM":
            {
                if (value is not float castValue) return false;
                instance.m_clutchSlipRPM = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
