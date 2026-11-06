// Automatically Generated

namespace Havok.hk2019.hkaiNavMeshGeneration;

public class SimpleGeometrySource : hkReferencedObject
{
    public hkGeometry? m_geometry;

    public List<hkaiNavMeshGeneration.SimpleGeometrySource.ThreeDVolume> m_threeDVolumes = new();

    public List<hkaiNavMeshGeneration.SimpleGeometrySource.AreaVolume> m_areaVolumes = new();


    public class AreaVolume : IHavokObject
    {
        public List<Vector4> m_vertices = new();

        public uint m_material;

    }

    public class ThreeDVolume : IHavokObject
    {
        public hkGeometry? m_geometry;

        public uint m_material;

    }

}
