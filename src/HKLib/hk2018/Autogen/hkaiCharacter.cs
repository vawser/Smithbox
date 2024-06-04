// Automatically Generated

namespace HKLib.hk2018;

public class hkaiCharacter : hkReferencedObject
{
    public ulong m_userData;

    public Vector4 m_position = new();

    public Vector4 m_forward = new();

    public Vector4 m_velocity = new();

    public Vector4 m_up = new();

    public uint m_currentNavMeshFace;

    public uint m_currentNavVolumeCell;

    public float m_radius;

    public float m_desiredSpeed;

    public hkaiAdaptiveRanger m_adaptiveRanger = new();

    public hkaiAstarCostModifier? m_costModifier;

    public hkaiAstarEdgeFilter? m_edgeFilter;

    public uint m_agentFilterInfo;

    public hkaiAvoidanceProperties? m_avoidanceProperties;

    public float m_avoidanceState;

    public uint m_agentPriority;

    public ushort m_avoidanceType;

    public hkaiCharacter.AvoidanceEnabledMaskBits m_avoidanceEnabledMask;

    public hkaiCharacter.State m_state;

    public int m_layerIndex;


    public enum AvoidanceState : int
    {
        AVOIDANCE_SUCCESS = 0,
        AVOIDANCE_FAILURE = 1
    }

    [Flags]
    public enum AvoidanceEnabledMaskBits : int
    {
        AVOID_BOUNDARIES = 1,
        AVOID_CHARACTERS = 2,
        AVOID_OBSTACLES = 4,
        AVOID_NONE = 0,
        AVOID_ALL = 7
    }

    public enum State : int
    {
        STATE_NEEDS_NEW_PATH = 0,
        STATE_FOLLOWING_PATH = 1,
        STATE_SLOWING_TO_GOAL = 2,
        STATE_GOAL_REACHED = 3,
        STATE_PATH_FAILED = 4,
        STATE_WANDERED_OFF_PATH = 5,
        STATE_REPATHING_INCOMPLETE_PATH = 6,
        STATE_MANUAL_CONTROL = 7
    }

}

