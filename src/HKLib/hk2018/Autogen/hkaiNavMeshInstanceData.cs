// Automatically Generated

namespace HKLib.hk2018;

public class hkaiNavMeshInstanceData : hkReferencedObject
{
    public hkaiNavMesh? m_originalMesh;

    public List<int> m_edgeOrigToInstancedMap = new();

    public List<int> m_edgeInstancedToOrigMap = new();

    public List<int> m_faceOrigToInstancedMap = new();

    public List<int> m_faceInstancedToOrigMap = new();

    public List<hkaiNavMesh.Face> m_instancedFaces = new();

    public List<hkaiNavMesh.Edge> m_instancedEdges = new();

    public List<hkaiNavMesh.Face> m_ownedFaces = new();

    public List<hkaiNavMesh.Edge> m_ownedEdges = new();

    public List<Vector4> m_ownedVertices = new();

    public List<byte> m_faceFlags = new();

    public List<ushort> m_origEdgeOffsets = new();

    public List<int> m_instancedFaceData = new();

    public List<int> m_instancedEdgeData = new();

    public List<int> m_ownedFaceData = new();

    public List<int> m_ownedEdgeData = new();

    public int m_numGarbageFaces;

    public int m_numGarbageEdges;

    public List<int> m_faceMapping = new();

    public uint m_sectionUid;

    public int m_runtimeId;

    public int m_layerIndex;

    public hkHashMap<int, hkaiNavMeshInstanceData.FaceDynUserEdgeBases> m_dynUserEdgeBases = new();


    public class FaceDynUserEdgeBases : IHavokObject
    {
        public List<hkaiNavMesh.Edge> m_edges = new();

        public List<int> m_edgeData = new();

        public List<hkHandle<uint>> m_setIds = new();

    }


}

