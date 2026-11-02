// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingGetVerticalClearanceSegmentsBasisQuadriplanarInputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBasisQuadriplanarInput> 
{
    public hkaiNavMeshGenerationSnapshottingGetVerticalClearanceSegmentsBasisQuadriplanarInputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBasisQuadriplanarInput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aabb":
            case "aabb":
            {
                if (instance.m_aabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_basis":
            case "basis":
            {
                if (instance.m_basis is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_intersectionPlaneImplicit":
            case "intersectionPlaneImplicit":
            {
                if (instance.m_intersectionPlaneImplicit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_testHeight":
            case "testHeight":
            {
                if (instance.m_testHeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stepHeight":
            case "stepHeight":
            {
                if (instance.m_stepHeight is not TGet castValue) return false;
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
            case "m_aabb":
            case "aabb":
            {
                if (value is not HKLib.hk2019.hkAabb castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            case "m_basis":
            case "basis":
            {
                if (value is not HKLib.hk2019.hkaiClipper.BasisQuadriplanarInnerTypes.Basis castValue) return false;
                instance.m_basis = castValue;
                return true;
            }
            case "m_intersectionPlaneImplicit":
            case "intersectionPlaneImplicit":
            {
                if (value is not HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.ImplicitPlane castValue) return false;
                instance.m_intersectionPlaneImplicit = castValue;
                return true;
            }
            case "m_testHeight":
            case "testHeight":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_testHeight = castValue;
                return true;
            }
            case "m_stepHeight":
            case "stepHeight":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_stepHeight = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
