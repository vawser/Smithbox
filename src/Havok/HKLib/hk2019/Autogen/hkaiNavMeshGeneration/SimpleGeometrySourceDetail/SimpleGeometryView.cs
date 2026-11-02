// Automatically Generated

namespace HKLib.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail;

public class SimpleGeometryView : hkReferencedObject
{
    public hkaiNavMeshGeneration.SimpleGeometrySource? m_source;

    public List<hkFpMath.FpIntVec3_4> m_vertices = new();

    public List<hkaiNavMeshGeneration.SimpleGeometrySourceDetail.GeometryFaceKey> m_potentiallyWalkableFaceKeys = new();

    public hkcdSimdTree m_trianglesTree = new();

    public List<hkaiNavMeshGeneration.InputVolume?> m_threeDVolumes = new();

    public hkcdSimdTree m_threeDVolumesTree = new();

    public List<hkaiNavMeshGeneration.InputArea?> m_areaVolumes = new();

    public hkcdSimdTree m_areaVolumesTree = new();

    public Matrix4x4 m_domainToWorld = new();

}
