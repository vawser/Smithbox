// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingGetGeometryVolumeClippingSegmentsBasisQuadriplanarOutputSegmentData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBasisQuadriplanarOutput.Segment> 
{
    public hkaiNavMeshGenerationSnapshottingGetGeometryVolumeClippingSegmentsBasisQuadriplanarOutputSegmentData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBasisQuadriplanarOutput.Segment instance) : base(type, instance) {}

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
                if (value is not HKLib.hk2019.hkaiClipper.BasisQuadriplanarInnerTypes.Vec2 castValue) return false;
                instance.m_a = castValue;
                return true;
            }
            case "m_b":
            case "b":
            {
                if (value is not HKLib.hk2019.hkaiClipper.BasisQuadriplanarInnerTypes.Vec2 castValue) return false;
                instance.m_b = castValue;
                return true;
            }
            case "m_lineEqn":
            case "lineEqn":
            {
                if (value is not HKLib.hk2019.hkaiClipper.BasisQuadriplanarInnerTypes.LineEqn castValue) return false;
                instance.m_lineEqn = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
