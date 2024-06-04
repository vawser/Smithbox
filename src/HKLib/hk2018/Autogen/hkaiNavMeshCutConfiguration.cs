// Automatically Generated

namespace HKLib.hk2018;

public class hkaiNavMeshCutConfiguration : IHavokObject
{
    public int m_sectionId;

    public hkaiNavMesh? m_navMesh;

    public hkHashMap<int, hkaiNavMeshCutConfiguration.FaceInfo> m_faceInfos = new();

    public hkHashMap<int, hkaiNavMeshCutConfiguration.BigFaceInfo> m_bigFaceInfos = new();

    public hkHashMap<int, List<hkaiNavMeshCutConfiguration.DynamicUserEdge>> m_dynamicUserEdges = new();


    public class DynamicUserEdge : IHavokObject
    {
        public int m_oppFace;

        public int m_oppDynUserEdgeIdx;

    }


    public class BigFaceInfo : IHavokObject
    {
        public int m_stitchCount;

        public bool m_isCutFace;

        public bool m_hasDynamicUserEdges;

        public hkBitField m_edgeIsCut = new();

    }


    public class FaceInfo : IHavokObject
    {
        public uint m_storage;

    }


}

