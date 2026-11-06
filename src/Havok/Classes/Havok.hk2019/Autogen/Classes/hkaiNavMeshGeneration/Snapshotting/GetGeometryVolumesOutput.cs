// Automatically Generated

namespace Havok.hk2019.hkaiNavMeshGeneration.Snapshotting;

public class GetGeometryVolumesOutput : IHavokObject
{
    public List<hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumesOutput.Volume> m_volumes = new();


    public class Volume : IHavokObject
    {
        public hkHandle<uint> m_geometryVolumeKey = new();

        public hkHandle<uint> m_geometryVolumeMaterial = new();

    }

}
