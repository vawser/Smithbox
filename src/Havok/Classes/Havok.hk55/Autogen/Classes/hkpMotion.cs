// Automatically Generated

namespace Havok.hk55;

public class hkpMotion : hkReferencedObject
{
    public hkpMotion.MotionType m_type;

    public byte m_deactivationIntegrateCounter;

    public readonly ushort[] m_deactivationNumInactiveFrames = new ushort[2];

    public hkMotionState m_motionState = new();

    public Vector4 m_inertiaAndMassInv = new();

    public Vector4 m_linearVelocity = new();

    public Vector4 m_angularVelocity = new();

    public readonly Vector4[] m_deactivationRefPosition = new Vector4[2];

    public readonly uint[] m_deactivationRefOrientation = new uint[2];

    public hkpMaxSizeMotion? m_savedMotion;

    public ushort m_savedQualityTypeIndex;


    public enum MotionType : int
    {
        MOTION_INVALID = 0,
        MOTION_DYNAMIC = 1,
        MOTION_SPHERE_INERTIA = 2,
        MOTION_STABILIZED_SPHERE_INERTIA = 3,
        MOTION_BOX_INERTIA = 4,
        MOTION_STABILIZED_BOX_INERTIA = 5,
        MOTION_KEYFRAMED = 6,
        MOTION_FIXED = 7,
        MOTION_THIN_BOX_INERTIA = 8,
        MOTION_CHARACTER = 9,
        MOTION_MAX_ID = 10
    }

}
