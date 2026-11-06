// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGetVerticalClearanceSegmentsBasisQuadriplanarInputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBasisQuadriplanarInput>
{
    public hkaiNavMeshGenerationSnapshottingGetVerticalClearanceSegmentsBasisQuadriplanarInputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBasisQuadriplanarInput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aabb":
            case "aabb":
            {
                value = instance.m_aabb;
                return true;
            }
            case "m_basis":
            case "basis":
            {
                value = instance.m_basis;
                return true;
            }
            case "m_intersectionPlaneImplicit":
            case "intersectionPlaneImplicit":
            {
                value = instance.m_intersectionPlaneImplicit;
                return true;
            }
            case "m_testHeight":
            case "testHeight":
            {
                value = instance.m_testHeight;
                return true;
            }
            case "m_stepHeight":
            case "stepHeight":
            {
                value = instance.m_stepHeight;
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
            case "m_aabb":
            case "aabb":
            {
                if (value is not Havok.hk2019.hkAabb castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            case "m_basis":
            case "basis":
            {
                if (value is not Havok.hk2019.hkaiClipper.BasisQuadriplanarInnerTypes.Basis castValue) return false;
                instance.m_basis = castValue;
                return true;
            }
            case "m_intersectionPlaneImplicit":
            case "intersectionPlaneImplicit":
            {
                if (value is not Havok.hk2019.hkaiClipper.BiplanarInnerTypes.ImplicitPlane castValue) return false;
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
