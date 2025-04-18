// Automatically Generated

namespace HKLib.hk2018.hkaiCollisionAvoidance;

public class Character : hkReferencedObject
{
    public ulong m_userData;

    public ulong m_boundaryGathererData;

    public Vector4 m_position = new();

    public Vector4 m_velocity = new();

    public Vector4 m_surfaceVelocity = new();

    public float m_distanceToLocalGoal;

    public Vector4 m_localGoalPlane = new();

    public Vector4 m_desiredDirection = new();

    public Vector4 m_avoidanceVelocity = new();

    public float m_radius;

    public float m_maximumSpeed;

    public float m_preferredSpeed;

    public hkaiCollisionAvoidance.Character.SensorSize m_sensorSize = new();

    public int m_maximumAvoidanceCharacters;

    public hkaiCollisionAvoidance.SteeringProperties m_steeringProperties = new();

    public hkaiCollisionAvoidance.BoundaryGatherer? m_boundaryGatherer;

    public List<hkaiCollisionAvoidance.ReferencedScoreModifier?> m_scoreModifiers = new();

    public bool m_steeringEnabled;

    public hkaiCollisionAvoidance.System? m_system;

    public int m_indexInSystem;

    public int m_enabledIndexInSystem;


    public class SensorSize : IHavokObject
    {
        public float m_halfWidth;

        public float m_topExtent;

        public float m_bottomExtent;

    }


}

