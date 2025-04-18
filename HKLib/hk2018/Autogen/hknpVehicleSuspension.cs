// Automatically Generated

namespace HKLib.hk2018;

public class hknpVehicleSuspension : hkReferencedObject
{
    public List<hknpVehicleSuspension.SuspensionWheelParameters> m_wheelParams = new();


    public class SuspensionWheelParameters : IHavokObject
    {
        public Vector4 m_hardpointChassisSpace = new();

        public Vector4 m_directionChassisSpace = new();

        public float m_length;

    }


}

