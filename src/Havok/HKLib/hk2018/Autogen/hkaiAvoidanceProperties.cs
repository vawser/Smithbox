// Automatically Generated

namespace HKLib.hk2018;

public class hkaiAvoidanceProperties : hkReferencedObject
{
    public hkaiMovementProperties m_movementProperties = new();

    public hkaiAvoidanceProperties.NearbyBoundariesSearchType m_nearbyBoundariesSearchType;

    public hkAabb m_localSensorAabb = new();

    public float m_wallFollowingAngle;

    public float m_dodgingPenalty;

    public float m_velocityHysteresis;

    public float m_sidednessChangingPenalty;

    public float m_collisionPenalty;

    public float m_penetrationPenalty;

    public int m_maxNeighbors;


    public enum NearbyBoundariesSearchType : int
    {
        SEARCH_NEIGHBORS = 0,
        SEARCH_FLOOD_FILL = 1
    }

}

