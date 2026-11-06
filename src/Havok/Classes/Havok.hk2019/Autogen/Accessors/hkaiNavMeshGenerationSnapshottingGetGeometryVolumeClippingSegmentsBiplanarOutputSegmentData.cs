// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGetGeometryVolumeClippingSegmentsBiplanarOutputSegmentData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBiplanarOutput.Segment>
{
    public hkaiNavMeshGenerationSnapshottingGetGeometryVolumeClippingSegmentsBiplanarOutputSegmentData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBiplanarOutput.Segment instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_a":
            case "a":
            {
                value = instance.m_a;
                return true;
            }
            case "m_b":
            case "b":
            {
                value = instance.m_b;
                return true;
            }
            case "m_lineEqn":
            case "lineEqn":
            {
                value = instance.m_lineEqn;
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
            case "m_a":
            case "a":
            {
                if (value is not Havok.hk2019.hkaiClipper.BiplanarInnerTypes.Vec2 castValue) return false;
                instance.m_a = castValue;
                return true;
            }
            case "m_b":
            case "b":
            {
                if (value is not Havok.hk2019.hkaiClipper.BiplanarInnerTypes.Vec2 castValue) return false;
                instance.m_b = castValue;
                return true;
            }
            case "m_lineEqn":
            case "lineEqn":
            {
                if (value is not Havok.hk2019.hkaiClipper.BiplanarInnerTypes.LineEqn castValue) return false;
                instance.m_lineEqn = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
