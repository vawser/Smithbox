// Automatically Generated

namespace HKLib.hk2019.hkaiViewerNodes;

public class Navigator : IHavokObject
{
    public int m_index;

    public float m_characterRadius;

    public bool m_hasEdgeFilter;

    public bool m_hasCostModifier;

    public uint m_filterInfo;

    public int m_layerIndex;

    public hkHandle<byte> m_clearanceCacheIdentifier = new();

    public bool m_hasCapsuleLimit;

    public bool m_hasLengthLimit;

    public int m_iterationLimit;

    public float m_searchRadius;

    public float m_horizontalTolerance;

    public float m_verticalTolerance;

    public bool m_usesHierarchicalHeuristic;

    public float m_heuristicWeight;

    public bool m_internalVertexPullingEnabled;

    public float m_goalReachedDistance;

    public float m_userEdgeEnteredDistance;

    public float m_leftTurningRadiusMultiplier;

    public float m_rightTurningRadiusMultiplier;

    public List<Vector4> m_goals = new();

    public Vector4 m_position = new();

    public hkaiNavigator.NavigationOutput m_navigationOutput = new();

}
