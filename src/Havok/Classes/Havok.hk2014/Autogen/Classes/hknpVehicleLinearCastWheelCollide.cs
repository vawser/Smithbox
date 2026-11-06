// Automatically Generated

namespace Havok.hk2014;

public class hknpVehicleLinearCastWheelCollide : hknpVehicleWheelCollide
{
    public List<hknpVehicleLinearCastWheelCollide.WheelState> m_wheelStates = new();

    public float m_maxExtraPenetration;

    public float m_startPointTolerance;

    public uint m_chassisBody;


    public class WheelState : IHavokObject
    {
        public hkAabb m_aabb = new();

        public hknpShape? m_shape;

        public Matrix4x4 m_transform = new();

        public Vector4 m_to = new();

    }

}
