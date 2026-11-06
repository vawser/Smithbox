// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGetGeometryVolumeClippingSegmentsBasisQuadriplanarOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBasisQuadriplanarOutput>
{
    public hkaiNavMeshGenerationSnapshottingGetGeometryVolumeClippingSegmentsBasisQuadriplanarOutputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBasisQuadriplanarOutput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_segments":
            case "segments":
            {
                value = instance.m_segments;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_segments":
            case "segments":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBasisQuadriplanarOutput.Segment> castValue) return false;
                instance.m_segments = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
