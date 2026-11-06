// Automatically Generated

namespace Havok.hk2014;

public class hknpMotion : IHavokObject
{
    public Vector4 m_centerOfMassAndMassFactor = new();

    public Quaternion m_orientation = new();

    public readonly float[] m_inverseInertia = new float[4];

    public uint m_firstAttachedBodyId;

    public readonly float[] m_linearVelocityCage = new float[3];

    public float m_integrationFactor;

    public ushort m_motionPropertiesId;

    public float m_maxLinearAccelerationDistancePerStep;

    public float m_maxRotationToPreventTunneling;

    public byte m_cellIndex;

    public byte m_spaceSplitterWeight;

    public Vector4 m_linearVelocity = new();

    public Vector4 m_angularVelocity = new();

    public Vector4 m_previousStepLinearVelocity = new();

    public Vector4 m_previousStepAngularVelocity = new();

}
