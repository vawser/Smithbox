// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGetGeometryVolumeClippingSegmentsBasisQuadriplanarInputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBasisQuadriplanarInput>
{
    public hkaiNavMeshGenerationSnapshottingGetGeometryVolumeClippingSegmentsBasisQuadriplanarInputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBasisQuadriplanarInput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometryVolumeKey":
            case "geometryVolumeKey":
            {
                value = instance.m_geometryVolumeKey;
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
            case "m_bottomExtrusion":
            case "bottomExtrusion":
            {
                value = instance.m_bottomExtrusion;
                return true;
            }
            case "m_topExtrusion":
            case "topExtrusion":
            {
                value = instance.m_topExtrusion;
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
            case "m_geometryVolumeKey":
            case "geometryVolumeKey":
            {
                if (value is not Havok.hk2019.hkHandle<uint> castValue) return false;
                instance.m_geometryVolumeKey = castValue;
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
            case "m_bottomExtrusion":
            case "bottomExtrusion":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_bottomExtrusion = castValue;
                return true;
            }
            case "m_topExtrusion":
            case "topExtrusion":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_topExtrusion = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
