// Automatically Generated

namespace HKLib.hk2018;

public class hknpMotion : IHavokObject
{
    public Vector4 m_centerOfMass = new();

    public Quaternion m_orientation = new();

    public readonly float[] m_inverseInertia = new float[4];

    public hknpBodyId m_firstAttachedBodyId = new();

    public readonly float[] m_linearVelocityCage = new float[3];

    public float m_integrationFactor;

    public ushort m_motionPropertiesId;

    public float m_lookAheadDistance;

    public float m_maxRotationPerStep;

    public byte m_cellIndex;

    public byte m_spaceSplitterWeight;

    public Vector4 m_linearVelocityAndSpeedLimit = new();

    public Vector4 m_angularVelocityLocalAndSpeedLimit = new();

    public Vector4 m_previousStepLinearVelocity = new();

    public Vector4 m_previousStepAngularVelocityLocal = new();

}

