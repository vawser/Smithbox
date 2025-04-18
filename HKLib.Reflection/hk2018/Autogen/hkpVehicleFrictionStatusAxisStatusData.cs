// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpVehicleFrictionStatusAxisStatusData : HavokData<hkpVehicleFrictionStatus.AxisStatus> 
{
    public hkpVehicleFrictionStatusAxisStatusData(HavokType type, hkpVehicleFrictionStatus.AxisStatus instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_forward_slip_velocity":
            case "forward_slip_velocity":
            {
                if (instance.m_forward_slip_velocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_side_slip_velocity":
            case "side_slip_velocity":
            {
                if (instance.m_side_slip_velocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_skid_energy_density":
            case "skid_energy_density":
            {
                if (instance.m_skid_energy_density is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_side_force":
            case "side_force":
            {
                if (instance.m_side_force is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_delayed_forward_impulse":
            case "delayed_forward_impulse":
            {
                if (instance.m_delayed_forward_impulse is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sideRhs":
            case "sideRhs":
            {
                if (instance.m_sideRhs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_forwardRhs":
            case "forwardRhs":
            {
                if (instance.m_forwardRhs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_relativeSideForce":
            case "relativeSideForce":
            {
                if (instance.m_relativeSideForce is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_relativeForwardForce":
            case "relativeForwardForce":
            {
                if (instance.m_relativeForwardForce is not TGet castValue) return false;
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
            case "m_forward_slip_velocity":
            case "forward_slip_velocity":
            {
                if (value is not float castValue) return false;
                instance.m_forward_slip_velocity = castValue;
                return true;
            }
            case "m_side_slip_velocity":
            case "side_slip_velocity":
            {
                if (value is not float castValue) return false;
                instance.m_side_slip_velocity = castValue;
                return true;
            }
            case "m_skid_energy_density":
            case "skid_energy_density":
            {
                if (value is not float castValue) return false;
                instance.m_skid_energy_density = castValue;
                return true;
            }
            case "m_side_force":
            case "side_force":
            {
                if (value is not float castValue) return false;
                instance.m_side_force = castValue;
                return true;
            }
            case "m_delayed_forward_impulse":
            case "delayed_forward_impulse":
            {
                if (value is not float castValue) return false;
                instance.m_delayed_forward_impulse = castValue;
                return true;
            }
            case "m_sideRhs":
            case "sideRhs":
            {
                if (value is not float castValue) return false;
                instance.m_sideRhs = castValue;
                return true;
            }
            case "m_forwardRhs":
            case "forwardRhs":
            {
                if (value is not float castValue) return false;
                instance.m_forwardRhs = castValue;
                return true;
            }
            case "m_relativeSideForce":
            case "relativeSideForce":
            {
                if (value is not float castValue) return false;
                instance.m_relativeSideForce = castValue;
                return true;
            }
            case "m_relativeForwardForce":
            case "relativeForwardForce":
            {
                if (value is not float castValue) return false;
                instance.m_relativeForwardForce = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
