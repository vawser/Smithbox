// Automatically Generated

namespace Havok.hk2019.hkaiNavMeshGeneration.Snapshotting;

public class ProcessGeometryFaceOutput : IHavokObject
{
    public bool m_clipFace;

    public hkHandle<uint> m_geometryFaceData = new();

    public List<hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput.AddVerticalClearanceTest> m_addVerticalClearanceTests = new();

    public List<hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput.AddGeometryVolume> m_addGeometryVolumes = new();


    public class AddGeometryVolume : IHavokObject
    {
        public hkHandle<uint> m_geometryVolumeKey = new();

        public float m_extrusion;

        public hkHandle<uint> m_geometryVolumeData = new();

    }

    public class AddVerticalClearanceTest : IHavokObject
    {
        public float m_height;

        public hkHandle<uint> m_data = new();

    }

}
