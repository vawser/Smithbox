// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpVehicleDefaultEngineData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpVehicleDefaultEngine>
{
    public hknpVehicleDefaultEngineData(Havok.hk2014.hknpVehicleDefaultEngine instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_minRPM":
            case "minRPM":
            {
                value = instance.m_minRPM;
                return true;
            }
            case "m_optRPM":
            case "optRPM":
            {
                value = instance.m_optRPM;
                return true;
            }
            case "m_maxRPM":
            case "maxRPM":
            {
                value = instance.m_maxRPM;
                return true;
            }
            case "m_maxTorque":
            case "maxTorque":
            {
                value = instance.m_maxTorque;
                return true;
            }
            case "m_torqueFactorAtMinRPM":
            case "torqueFactorAtMinRPM":
            {
                value = instance.m_torqueFactorAtMinRPM;
                return true;
            }
            case "m_torqueFactorAtMaxRPM":
            case "torqueFactorAtMaxRPM":
            {
                value = instance.m_torqueFactorAtMaxRPM;
                return true;
            }
            case "m_resistanceFactorAtMinRPM":
            case "resistanceFactorAtMinRPM":
            {
                value = instance.m_resistanceFactorAtMinRPM;
                return true;
            }
            case "m_resistanceFactorAtOptRPM":
            case "resistanceFactorAtOptRPM":
            {
                value = instance.m_resistanceFactorAtOptRPM;
                return true;
            }
            case "m_resistanceFactorAtMaxRPM":
            case "resistanceFactorAtMaxRPM":
            {
                value = instance.m_resistanceFactorAtMaxRPM;
                return true;
            }
            case "m_clutchSlipRPM":
            case "clutchSlipRPM":
            {
                value = instance.m_clutchSlipRPM;
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
