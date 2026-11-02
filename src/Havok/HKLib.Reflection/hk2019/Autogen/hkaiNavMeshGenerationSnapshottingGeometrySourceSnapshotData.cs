// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingGeometrySourceSnapshotData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometrySourceSnapshot> 
{
    public hkaiNavMeshGenerationSnapshottingGeometrySourceSnapshotData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometrySourceSnapshot instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_getGeometryFacesOutput":
            case "getGeometryFacesOutput":
            {
                if (instance.m_getGeometryFacesOutput is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_getGeometryFaceAabbRecords":
            case "getGeometryFaceAabbRecords":
            {
                if (instance.m_getGeometryFaceAabbRecords is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_getGeometryFaceMaterialRecords":
            case "getGeometryFaceMaterialRecords":
            {
                if (instance.m_getGeometryFaceMaterialRecords is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_getGeometryFaceVerticesRecords":
            case "getGeometryFaceVerticesRecords":
            {
                if (instance.m_getGeometryFaceVerticesRecords is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_getVerticalClearanceSegmentsBiplanarRecords":
            case "getVerticalClearanceSegmentsBiplanarRecords":
            {
                if (instance.m_getVerticalClearanceSegmentsBiplanarRecords is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_getVerticalClearanceSegmentsBasisQuadriplanarRecords":
            case "getVerticalClearanceSegmentsBasisQuadriplanarRecords":
            {
                if (instance.m_getVerticalClearanceSegmentsBasisQuadriplanarRecords is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_getGeometryVolumesRecords":
            case "getGeometryVolumesRecords":
            {
                if (instance.m_getGeometryVolumesRecords is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_getGeometryVolumeClippingSegmentsBiplanarRecords":
            case "getGeometryVolumeClippingSegmentsBiplanarRecords":
            {
                if (instance.m_getGeometryVolumeClippingSegmentsBiplanarRecords is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_getGeometryVolumeClippingSegmentsBasisQuadriplanarRecords":
            case "getGeometryVolumeClippingSegmentsBasisQuadriplanarRecords":
            {
                if (instance.m_getGeometryVolumeClippingSegmentsBasisQuadriplanarRecords is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_geometryFacesIntersectXyPrismRecords":
            case "geometryFacesIntersectXyPrismRecords":
            {
                if (instance.m_geometryFacesIntersectXyPrismRecords is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_geometryVolumeIntersectsPointRecords":
            case "geometryVolumeIntersectsPointRecords":
            {
                if (instance.m_geometryVolumeIntersectsPointRecords is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_getTrimeshOutput":
            case "getTrimeshOutput":
            {
                if (instance.m_getTrimeshOutput is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_getTrimeshTrianglesForGeometryFaceRecords":
            case "getTrimeshTrianglesForGeometryFaceRecords":
            {
                if (instance.m_getTrimeshTrianglesForGeometryFaceRecords is not TGet castValue) return false;
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
            case "m_getGeometryFacesOutput":
            case "getGeometryFacesOutput":
            {
                if (value is not HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFacesOutput castValue) return false;
                instance.m_getGeometryFacesOutput = castValue;
                return true;
            }
            case "m_getGeometryFaceAabbRecords":
            case "getGeometryFaceAabbRecords":
            {
                if (value is not HKLib.hk2019.hkHashMap<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFaceAabbInput, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFaceAabbOutput> castValue) return false;
                instance.m_getGeometryFaceAabbRecords = castValue;
                return true;
            }
            case "m_getGeometryFaceMaterialRecords":
            case "getGeometryFaceMaterialRecords":
            {
                if (value is not HKLib.hk2019.hkHashMap<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFaceMaterialInput, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFaceMaterialOutput> castValue) return false;
                instance.m_getGeometryFaceMaterialRecords = castValue;
                return true;
            }
            case "m_getGeometryFaceVerticesRecords":
            case "getGeometryFaceVerticesRecords":
            {
                if (value is not HKLib.hk2019.hkHashMap<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFaceVerticesInput, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFaceVerticesOutput> castValue) return false;
                instance.m_getGeometryFaceVerticesRecords = castValue;
                return true;
            }
            case "m_getVerticalClearanceSegmentsBiplanarRecords":
            case "getVerticalClearanceSegmentsBiplanarRecords":
            {
                if (value is not HKLib.hk2019.hkHashMap<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBiplanarInput, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBiplanarOutput> castValue) return false;
                instance.m_getVerticalClearanceSegmentsBiplanarRecords = castValue;
                return true;
            }
            case "m_getVerticalClearanceSegmentsBasisQuadriplanarRecords":
            case "getVerticalClearanceSegmentsBasisQuadriplanarRecords":
            {
                if (value is not HKLib.hk2019.hkHashMap<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBasisQuadriplanarInput, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBasisQuadriplanarOutput> castValue) return false;
                instance.m_getVerticalClearanceSegmentsBasisQuadriplanarRecords = castValue;
                return true;
            }
            case "m_getGeometryVolumesRecords":
            case "getGeometryVolumesRecords":
            {
                if (value is not HKLib.hk2019.hkHashMap<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumesInput, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumesOutput> castValue) return false;
                instance.m_getGeometryVolumesRecords = castValue;
                return true;
            }
            case "m_getGeometryVolumeClippingSegmentsBiplanarRecords":
            case "getGeometryVolumeClippingSegmentsBiplanarRecords":
            {
                if (value is not HKLib.hk2019.hkHashMap<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBiplanarInput, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBiplanarOutput> castValue) return false;
                instance.m_getGeometryVolumeClippingSegmentsBiplanarRecords = castValue;
                return true;
            }
            case "m_getGeometryVolumeClippingSegmentsBasisQuadriplanarRecords":
            case "getGeometryVolumeClippingSegmentsBasisQuadriplanarRecords":
            {
                if (value is not HKLib.hk2019.hkHashMap<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBasisQuadriplanarInput, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBasisQuadriplanarOutput> castValue) return false;
                instance.m_getGeometryVolumeClippingSegmentsBasisQuadriplanarRecords = castValue;
                return true;
            }
            case "m_geometryFacesIntersectXyPrismRecords":
            case "geometryFacesIntersectXyPrismRecords":
            {
                if (value is not HKLib.hk2019.hkHashMap<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryFacesIntersectXyPrismInput, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryFacesIntersectXyPrismOutput> castValue) return false;
                instance.m_geometryFacesIntersectXyPrismRecords = castValue;
                return true;
            }
            case "m_geometryVolumeIntersectsPointRecords":
            case "geometryVolumeIntersectsPointRecords":
            {
                if (value is not HKLib.hk2019.hkHashMap<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryVolumeIntersectsPointInput, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryVolumeIntersectsPointOutput> castValue) return false;
                instance.m_geometryVolumeIntersectsPointRecords = castValue;
                return true;
            }
            case "m_getTrimeshOutput":
            case "getTrimeshOutput":
            {
                if (value is not HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetTrimeshOutput castValue) return false;
                instance.m_getTrimeshOutput = castValue;
                return true;
            }
            case "m_getTrimeshTrianglesForGeometryFaceRecords":
            case "getTrimeshTrianglesForGeometryFaceRecords":
            {
                if (value is not HKLib.hk2019.hkHashMap<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetTrimeshTrianglesForGeometryFaceInput, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetTrimeshTrianglesForGeometryFaceOutput> castValue) return false;
                instance.m_getTrimeshTrianglesForGeometryFaceRecords = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
