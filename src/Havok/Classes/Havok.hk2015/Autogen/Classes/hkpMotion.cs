// Automatically Generated

namespace Havok.hk2015;

public class hkpMotion : hkReferencedObject
{
    public byte m_type;

    public byte m_deactivationIntegrateCounter;

    public readonly ushort[] m_deactivationNumInactiveFrames = new ushort[2];

    public hkMotionState m_motionState = new();

    public Vector4 m_inertiaAndMassInv = new();

    public Vector4 m_linearVelocity = new();

    public Vector4 m_angularVelocity = new();

    public readonly Vector4[] m_deactivationRefPosition = new Vector4[2];

    public readonly uint[] m_deactivationRefOrientation = new uint[2];

    public hkpMotion? m_savedMotion;

    public ushort m_savedQualityTypeIndex;

    public float m_gravityFactor;

}
