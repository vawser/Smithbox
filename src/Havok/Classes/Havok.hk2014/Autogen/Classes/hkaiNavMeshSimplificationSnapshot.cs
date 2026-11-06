// Automatically Generated

namespace Havok.hk2014;

public class hkaiNavMeshSimplificationSnapshot : IHavokObject
{
    public hkGeometry? m_geometry;

    public List<hkaiCarver?> m_carvers = new();

    public hkBitField m_cuttingTriangles = new();

    public hkaiNavMeshGeneration.Settings m_settings = new();

    public hkaiNavMesh? m_unsimplifiedNavMesh;

}
