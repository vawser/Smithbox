// Automatically Generated

namespace HKLib.hk2018;

public class hkaiPathFollowingBehavior : hkaiSingleCharacterBehavior, hkaiPathRequestInfoOwner
{
    public hkaiPathFollowingProperties? m_pathFollowingProperties;

    public hkaiPath? m_currentPath;

    public hkaiPath? m_currentPathFixed;

    public int m_currentPathSegment;

    public int m_previousPathSegment;

    public int m_newCharacterState;

    public float m_changeSegmentDistance;

    public float m_tempChangeSegmentDistance;

    public float m_updateQuerySize;

    public float m_characterRadiusMultiplier;

    public float m_characterToPathStartThreshold;

    public bool m_useSectionLocalPaths;

    public hkaiPathFollowingBehavior.PathType m_pathType;

    public bool m_lastPointIsGoal;

    public bool m_needsRepath;

    public bool m_passiveAvoidance;

    public hkaiCharacter.State m_savedCharacterState;


    public enum PathType : int
    {
        PATH_TYPE_NAVMESH = 0,
        PATH_TYPE_NAVMESH_CLIMBING = 1,
        PATH_TYPE_NAVVOLUME = 2
    }

}

