// Automatically Generated

namespace HKLib.hk2018;

public class hkpVehicleFrictionDescription : hkReferencedObject
{
    public float m_wheelDistance;

    public float m_chassisMassInv;

    public readonly hkpVehicleFrictionDescription.AxisDescription[] m_axleDescr = new hkpVehicleFrictionDescription.AxisDescription[2];


    public class AxisDescription : IHavokObject
    {
        public readonly float[] m_frictionCircleYtab = new float[16];

        public float m_xStep;

        public float m_xStart;

        public float m_wheelSurfaceInertia;

        public float m_wheelSurfaceInertiaInv;

        public float m_wheelChassisMassRatio;

        public float m_wheelRadius;

        public float m_wheelRadiusInv;

        public float m_wheelDownForceFactor;

        public float m_wheelDownForceSumFactor;

    }


}

