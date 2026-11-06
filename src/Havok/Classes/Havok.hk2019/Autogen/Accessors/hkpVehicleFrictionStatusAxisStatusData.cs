// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkpVehicleFrictionStatusAxisStatusData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkpVehicleFrictionStatus.AxisStatus>
{
    public hkpVehicleFrictionStatusAxisStatusData(Havok.hk2019.hkpVehicleFrictionStatus.AxisStatus instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_forward_slip_velocity":
            case "forward_slip_velocity":
            {
                value = instance.m_forward_slip_velocity;
                return true;
            }
            case "m_side_slip_velocity":
            case "side_slip_velocity":
            {
                value = instance.m_side_slip_velocity;
                return true;
            }
            case "m_skid_energy_density":
            case "skid_energy_density":
            {
                value = instance.m_skid_energy_density;
                return true;
            }
            case "m_side_force":
            case "side_force":
            {
                value = instance.m_side_force;
                return true;
            }
            case "m_delayed_forward_impulse":
            case "delayed_forward_impulse":
            {
                value = instance.m_delayed_forward_impulse;
                return true;
            }
            case "m_sideRhs":
            case "sideRhs":
            {
                value = instance.m_sideRhs;
                return true;
            }
            case "m_forwardRhs":
            case "forwardRhs":
            {
                value = instance.m_forwardRhs;
                return true;
            }
            case "m_relativeSideForce":
            case "relativeSideForce":
            {
                value = instance.m_relativeSideForce;
                return true;
            }
            case "m_relativeForwardForce":
            case "relativeForwardForce":
            {
                value = instance.m_relativeForwardForce;
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
