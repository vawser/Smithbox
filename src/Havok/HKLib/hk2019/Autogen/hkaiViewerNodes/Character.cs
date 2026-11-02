// Automatically Generated

namespace HKLib.hk2019.hkaiViewerNodes;

public class Character : IHavokObject
{
    public int m_index;

    public int m_enabledIndex;

    public float m_radius;

    public float m_maximumSpeed;

    public float m_preferredSpeed;

    public hkaiCollisionAvoidance.Character.SensorSize m_sensorSize = new();

    public int m_maximumAvoidanceCharacters;

    public hkaiCollisionAvoidance.SteeringProperties m_steeringProperties = new();

    public Vector4 m_position = new();

    public Vector4 m_currentVelocity = new();

    public Vector4 m_surfaceVelocity = new();

    public bool m_steeringEnabled;

    public Vector4 m_desiredDirection = new();

    public Vector4 m_avoidanceVelocity = new();

    public ulong m_userData;

}
