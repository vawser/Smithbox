// Automatically Generated

namespace Havok.hk2014;

public class hkaiNavMeshCutter : hkReferencedObject
{
    public List<hkaiNavMeshCutterMeshInfo> m_meshInfos = new();

    public hkaiNavMeshCutterSavedConnectivity m_connectivityInfo = new();

    public hkaiStreamingCollection? m_streamingCollection;

    public List<uint> m_forceRecutFaceKeys = new();

    public List<uint> m_forceClearanceCalcFaceKeys = new();

    public Vector4 m_up = new();

    public hkaiNavMeshEdgeMatchingParameters m_edgeMatchParams = new();

    public float m_cutEdgeTolerance;

    public float m_minEdgeMatchingLength;

    public float m_smallGapFixupTolerance;

    public bool m_performValidationChecks;

    public hkaiNavMeshCutter.ClearanceResetMethod m_clearanceResetMethod;

    public bool m_recomputeClearanceAfterCutting;

    public bool m_useNewCutter;

    public float m_domainQuantum;


    public enum ClearanceResetMethod : int
    {
        CLEARANCE_RESET_ALL = 0,
        CLEARANCE_RESET_MEDIATOR = 1,
        CLEARANCE_RESET_FLOODFILL = 2
    }

}
