// Automatically Generated

namespace HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting;

public class ProcessGeometryFragmentInput : IHavokObject
{
    public hkHandle<uint> m_geometryFaceKey = new();

    public int m_fragmentIndex;

    public hkHandle<uint> m_geometryFaceData = new();

    public hkHandle<uint> m_verticalClearanceTestData = new();

    public List<hkHandle<uint>> m_geometryVolumeDatas = new();

}
