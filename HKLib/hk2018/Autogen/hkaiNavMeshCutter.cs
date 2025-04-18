// Automatically Generated

namespace HKLib.hk2018;

public class hkaiNavMeshCutter : hkReferencedObject
{
    public hkaiStreamingCollection? m_streamingCollection;

    public List<hkaiNavMeshInstanceCutter?> m_instanceCutters = new();

    public hkaiNavMeshCompactUtils.CompactingState m_compactingState = new();

    public Vector4 m_up = new();

    public hkaiNavMeshEdgeMatchingParameters m_edgeMatchParams = new();

    public float m_cutEdgeTolerance;

    public float m_minEdgeMatchingLength;

    public hkaiWorld.ClearanceResetMethod m_clearanceResetMethod;

    public float m_smallGapFixupTolerance;

    public bool m_performValidationChecks;

    public float m_maxGarbageRatio;

    public float m_domainQuantum;

    public hkaiDefaultDynamicUserEdgeSetInfo m_defaultDynUserEdgeSetInfo = new();

    public hkHashMap<hkHandle<uint>, hkaiDynamicUserEdgeSetInfo> m_dynUserEdgeSetInfos = new();

    public hkaiModifiedSections? m_modifiedSections;

}

