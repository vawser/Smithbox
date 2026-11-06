// Automatically Generated

namespace Havok.hk2019.hkaiVolumePathfindingUtil;

public class FindPathInput : hkReferencedObject
{
    public Vector4 m_startPoint = new();

    public List<Vector4> m_goalPoints = new();

    public uint m_startCellKey;

    public List<uint> m_goalCellKeys = new();

    public int m_maxNumberOfIterations;

    public hkaiAgentTraversalInfo m_agentInfo = new();

    public hkaiNavVolumePathSearchParameters m_searchParameters = new();

}
