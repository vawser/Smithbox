// Automatically Generated

namespace HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting;

public class GetGeometryVolumeClippingSegmentsBasisQuadriplanarInput : IHavokObject
{
    public hkHandle<uint> m_geometryVolumeKey = new();

    public hkaiClipper.BasisQuadriplanarInnerTypes.Basis m_basis = new();

    public hkaiClipper.BiplanarInnerTypes.ImplicitPlane m_intersectionPlaneImplicit = new();

    public List<byte> m_bottomExtrusion = new();

    public List<byte> m_topExtrusion = new();

}
