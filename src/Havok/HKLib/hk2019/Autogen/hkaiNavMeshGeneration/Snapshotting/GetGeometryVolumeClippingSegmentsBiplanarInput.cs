// Automatically Generated

namespace HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting;

public class GetGeometryVolumeClippingSegmentsBiplanarInput : IHavokObject
{
    public hkHandle<uint> m_geometryVolumeKey = new();

    public hkaiClipper.BiplanarInnerTypes.Basis m_basis = new();

    public List<byte> m_bottomExtrusion = new();

    public List<byte> m_topExtrusion = new();

}
