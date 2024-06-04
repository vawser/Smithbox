// Automatically Generated

namespace HKLib.hk2018;

public class hkaiEdgeFollowingBehavior : hkaiSingleCharacterBehavior, hkaiPathRequestInfoOwner
{
    public float m_updateQuerySize;

    public float m_characterRadiusMultiplier;

    public float m_maxIgnoredHeight;

    public hkaiEdgePath? m_edgePath;

    public hkaiEdgePath.TraversalState m_traversalState = new();

    public hkaiCharacter.State m_newCharacterState;

    public hkaiPathFollowingProperties? m_pathFollowingProperties;

    public int m_highestUserEdgeNotified;

    public hkaiPath.PathPoint m_userEdgeFakePathPoint = new();

    public hkaiCharacter.State m_savedCharacterState;

    public hkaiEdgeFollowingBehavior.CornerPredictorInitInfo m_cornerPredictorInitInfo = new();

    public bool m_passiveAvoidance;

    public bool m_resubmitEarlyIterationTerminations;


    public class CornerPredictorInitInfo : IHavokObject
    {
        public Vector4 m_positionLocal = new();

        public Vector4 m_forwardVectorLocal = new();

        public Vector4 m_upLocal = new();

        public int m_positionSectionIndex;

        public int m_nextEdgeIndex;

        public bool m_nextIsLeft;

        public bool m_hasInfo;

    }


}

