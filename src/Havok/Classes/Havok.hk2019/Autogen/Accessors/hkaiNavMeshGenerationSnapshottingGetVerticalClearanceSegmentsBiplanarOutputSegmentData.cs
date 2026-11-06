// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGetVerticalClearanceSegmentsBiplanarOutputSegmentData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBiplanarOutput.Segment>
{
    public hkaiNavMeshGenerationSnapshottingGetVerticalClearanceSegmentsBiplanarOutputSegmentData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBiplanarOutput.Segment instance) : base(instance) { }

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
            case "m_fromExtrusion":
            case "fromExtrusion":
            {
                value = instance.m_fromExtrusion;
                return true;
            }
            case "m_toExtrusion":
            case "toExtrusion":
            {
                value = instance.m_toExtrusion;
                return true;
            }
            case "m_blocksConnection":
            case "blocksConnection":
            {
                value = instance.m_blocksConnection;
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
            case "m_fromExtrusion":
            case "fromExtrusion":
            {
                if (value is not int castValue) return false;
                instance.m_fromExtrusion = castValue;
                return true;
            }
            case "m_toExtrusion":
            case "toExtrusion":
            {
                if (value is not int castValue) return false;
                instance.m_toExtrusion = castValue;
                return true;
            }
            case "m_blocksConnection":
            case "blocksConnection":
            {
                if (value is not bool castValue) return false;
                instance.m_blocksConnection = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
