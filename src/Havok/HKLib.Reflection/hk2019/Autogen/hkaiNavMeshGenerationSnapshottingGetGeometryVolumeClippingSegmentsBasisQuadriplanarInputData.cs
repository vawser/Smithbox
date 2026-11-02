// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingGetGeometryVolumeClippingSegmentsBasisQuadriplanarInputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBasisQuadriplanarInput> 
{
    public hkaiNavMeshGenerationSnapshottingGetGeometryVolumeClippingSegmentsBasisQuadriplanarInputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBasisQuadriplanarInput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometryVolumeKey":
            case "geometryVolumeKey":
            {
                if (instance.m_geometryVolumeKey is not TGet castValue) return false;
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
            case "m_bottomExtrusion":
            case "bottomExtrusion":
            {
                if (instance.m_bottomExtrusion is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_topExtrusion":
            case "topExtrusion":
            {
                if (instance.m_topExtrusion is not TGet castValue) return false;
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
            case "m_geometryVolumeKey":
            case "geometryVolumeKey":
            {
                if (value is not HKLib.hk2019.hkHandle<uint> castValue) return false;
                instance.m_geometryVolumeKey = castValue;
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
