// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpVehicleDataWheelComponentParamsData : HavokData<hknpVehicleData.WheelComponentParams> 
{
    public hknpVehicleDataWheelComponentParamsData(HavokType type, hknpVehicleData.WheelComponentParams instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_radius":
            case "radius":
            {
                if (instance.m_radius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                if (instance.m_mass is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_width":
            case "width":
            {
                if (instance.m_width is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_friction":
            case "friction":
            {
                if (instance.m_friction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_viscosityFriction":
            case "viscosityFriction":
            {
                if (instance.m_viscosityFriction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxFriction":
            case "maxFriction":
            {
                if (instance.m_maxFriction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_slipAngle":
            case "slipAngle":
            {
                if (instance.m_slipAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_forceFeedbackMultiplier":
            case "forceFeedbackMultiplier":
            {
                if (instance.m_forceFeedbackMultiplier is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxContactBodyAcceleration":
            case "maxContactBodyAcceleration":
            {
                if (instance.m_maxContactBodyAcceleration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_axle":
            case "axle":
            {
                if (instance.m_axle is not TGet castValue) return false;
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
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                if (value is not float castValue) return false;
                instance.m_mass = castValue;
                return true;
            }
            case "m_width":
            case "width":
            {
                if (value is not float castValue) return false;
                instance.m_width = castValue;
                return true;
            }
            case "m_friction":
            case "friction":
            {
                if (value is not float castValue) return false;
                instance.m_friction = castValue;
                return true;
            }
            case "m_viscosityFriction":
            case "viscosityFriction":
            {
                if (value is not float castValue) return false;
                instance.m_viscosityFriction = castValue;
                return true;
            }
            case "m_maxFriction":
            case "maxFriction":
            {
                if (value is not float castValue) return false;
                instance.m_maxFriction = castValue;
                return true;
            }
            case "m_slipAngle":
            case "slipAngle":
            {
                if (value is not float castValue) return false;
                instance.m_slipAngle = castValue;
                return true;
            }
            case "m_forceFeedbackMultiplier":
            case "forceFeedbackMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_forceFeedbackMultiplier = castValue;
                return true;
            }
            case "m_maxContactBodyAcceleration":
            case "maxContactBodyAcceleration":
            {
                if (value is not float castValue) return false;
                instance.m_maxContactBodyAcceleration = castValue;
                return true;
            }
            case "m_axle":
            case "axle":
            {
                if (value is not sbyte castValue) return false;
                instance.m_axle = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
