// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingGetVerticalClearanceSegmentsBiplanarOutputSegmentData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBiplanarOutput.Segment> 
{
    public hkaiNavMeshGenerationSnapshottingGetVerticalClearanceSegmentsBiplanarOutputSegmentData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBiplanarOutput.Segment instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_a":
            case "a":
            {
                if (instance.m_a is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_b":
            case "b":
            {
                if (instance.m_b is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lineEqn":
            case "lineEqn":
            {
                if (instance.m_lineEqn is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fromExtrusion":
            case "fromExtrusion":
            {
                if (instance.m_fromExtrusion is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toExtrusion":
            case "toExtrusion":
            {
                if (instance.m_toExtrusion is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blocksConnection":
            case "blocksConnection":
            {
                if (instance.m_blocksConnection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_a":
            case "a":
            {
                if (value is not HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.Vec2 castValue) return false;
                instance.m_a = castValue;
                return true;
            }
            case "m_b":
            case "b":
            {
                if (value is not HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.Vec2 castValue) return false;
                instance.m_b = castValue;
                return true;
            }
            case "m_lineEqn":
            case "lineEqn":
            {
                if (value is not HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.LineEqn castValue) return false;
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
