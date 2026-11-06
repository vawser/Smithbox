// Automatically Generated

namespace Havok.hk2014;

public class hkaiNavMeshInstance : hkReferencedObject
{
    public hkaiNavMesh? m_originalMesh;

    public hkaiReferenceFrame m_referenceFrame = new();

    public List<int> m_edgeMap = new();

    public List<int> m_faceMap = new();

    public List<hkaiNavMesh.Face> m_instancedFaces = new();

    public List<hkaiNavMesh.Edge> m_instancedEdges = new();

    public List<hkaiNavMesh.Face> m_ownedFaces = new();

    public List<hkaiNavMesh.Edge> m_ownedEdges = new();

    public List<Vector4> m_ownedVertices = new();

    public List<byte> m_faceFlags = new();

    public List<ushort> m_cuttingInfo = new();

    public List<int> m_instancedFaceData = new();

    public List<int> m_instancedEdgeData = new();

    public List<int> m_ownedFaceData = new();

    public List<int> m_ownedEdgeData = new();

    public List<float> m_clearanceCache = new();

    public List<float> m_globalClearanceCache = new();

    public List<int> m_faceClearanceIndices = new();

    public float m_maxGlobalClearance;

    public uint m_sectionUid;

    public int m_runtimeId;

    public uint m_layer;

}
