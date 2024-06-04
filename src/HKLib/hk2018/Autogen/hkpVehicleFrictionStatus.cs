// Automatically Generated

namespace HKLib.hk2018;

public class hkpVehicleFrictionStatus : IHavokObject
{
    public readonly hkpVehicleFrictionStatus.AxisStatus[] m_axis = new hkpVehicleFrictionStatus.AxisStatus[2];


    public class AxisStatus : IHavokObject
    {
        public float m_forward_slip_velocity;

        public float m_side_slip_velocity;

        public float m_skid_energy_density;

        public float m_side_force;

        public float m_delayed_forward_impulse;

        public float m_sideRhs;

        public float m_forwardRhs;

        public float m_relativeSideForce;

        public float m_relativeForwardForce;

    }


}

