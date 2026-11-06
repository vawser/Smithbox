// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGetVerticalClearanceSegmentsBasisQuadriplanarOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBasisQuadriplanarOutput>
{
    public hkaiNavMeshGenerationSnapshottingGetVerticalClearanceSegmentsBasisQuadriplanarOutputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBasisQuadriplanarOutput instance) : base(instance) { }

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
                if (value is not List<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBasisQuadriplanarOutput.Segment> castValue) return false;
                instance.m_segments = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
