// Automatically Generated

namespace Havok.hk2019.hkaiNavMeshGeneration.Snapshotting;

public class GetVerticalClearanceSegmentsBiplanarInput : IHavokObject
{
    public hkHandle<uint> m_geometryFaceKey = new();

    public hkaiClipper.BiplanarInnerTypes.Basis m_basis = new();

    public List<List<byte>> m_testHeights = new();

    public List<byte> m_stepHeight = new();

}
