// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGeometrySourceSnapshotData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometrySourceSnapshot>
{
    public hkaiNavMeshGenerationSnapshottingGeometrySourceSnapshotData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometrySourceSnapshot instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_getGeometryFacesOutput":
            case "getGeometryFacesOutput":
            {
                value = instance.m_getGeometryFacesOutput;
                return true;
            }
            case "m_getGeometryFaceAabbRecords":
            case "getGeometryFaceAabbRecords":
            {
                value = instance.m_getGeometryFaceAabbRecords;
                return true;
            }
            case "m_getGeometryFaceMaterialRecords":
            case "getGeometryFaceMaterialRecords":
            {
                value = instance.m_getGeometryFaceMaterialRecords;
                return true;
            }
            case "m_getGeometryFaceVerticesRecords":
            case "getGeometryFaceVerticesRecords":
            {
                value = instance.m_getGeometryFaceVerticesRecords;
                return true;
            }
            case "m_getVerticalClearanceSegmentsBiplanarRecords":
            case "getVerticalClearanceSegmentsBiplanarRecords":
            {
                value = instance.m_getVerticalClearanceSegmentsBiplanarRecords;
                return true;
            }
            case "m_getVerticalClearanceSegmentsBasisQuadriplanarRecords":
            case "getVerticalClearanceSegmentsBasisQuadriplanarRecords":
            {
                value = instance.m_getVerticalClearanceSegmentsBasisQuadriplanarRecords;
                return true;
            }
            case "m_getGeometryVolumesRecords":
            case "getGeometryVolumesRecords":
            {
                value = instance.m_getGeometryVolumesRecords;
                return true;
            }
            case "m_getGeometryVolumeClippingSegmentsBiplanarRecords":
            case "getGeometryVolumeClippingSegmentsBiplanarRecords":
            {
                value = instance.m_getGeometryVolumeClippingSegmentsBiplanarRecords;
                return true;
            }
            case "m_getGeometryVolumeClippingSegmentsBasisQuadriplanarRecords":
            case "getGeometryVolumeClippingSegmentsBasisQuadriplanarRecords":
            {
                value = instance.m_getGeometryVolumeClippingSegmentsBasisQuadriplanarRecords;
                return true;
            }
            case "m_geometryFacesIntersectXyPrismRecords":
            case "geometryFacesIntersectXyPrismRecords":
            {
                value = instance.m_geometryFacesIntersectXyPrismRecords;
                return true;
            }
            case "m_geometryVolumeIntersectsPointRecords":
            case "geometryVolumeIntersectsPointRecords":
            {
                value = instance.m_geometryVolumeIntersectsPointRecords;
                return true;
            }
            case "m_getTrimeshOutput":
            case "getTrimeshOutput":
            {
                value = instance.m_getTrimeshOutput;
                return true;
            }
            case "m_getTrimeshTrianglesForGeometryFaceRecords":
            case "getTrimeshTrianglesForGeometryFaceRecords":
            {
                value = instance.m_getTrimeshTrianglesForGeometryFaceRecords;
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
            case "m_getGeometryFacesOutput":
            case "getGeometryFacesOutput":
            {
                if (value is not Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFacesOutput castValue) return false;
                instance.m_getGeometryFacesOutput = castValue;
                return true;
            }
            case "m_getGeometryFaceAabbRecords":
            case "getGeometryFaceAabbRecords":
            {
                if (value is not Havok.hk2019.hkHashMap<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFaceAabbInput, Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFaceAabbOutput> castValue) return false;
                instance.m_getGeometryFaceAabbRecords = castValue;
                return true;
            }
            case "m_getGeometryFaceMaterialRecords":
            case "getGeometryFaceMaterialRecords":
            {
                if (value is not Havok.hk2019.hkHashMap<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFaceMaterialInput, Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFaceMaterialOutput> castValue) return false;
                instance.m_getGeometryFaceMaterialRecords = castValue;
                return true;
            }
            case "m_getGeometryFaceVerticesRecords":
            case "getGeometryFaceVerticesRecords":
            {
                if (value is not Havok.hk2019.hkHashMap<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFaceVerticesInput, Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFaceVerticesOutput> castValue) return false;
                instance.m_getGeometryFaceVerticesRecords = castValue;
                return true;
            }
            case "m_getVerticalClearanceSegmentsBiplanarRecords":
            case "getVerticalClearanceSegmentsBiplanarRecords":
            {
                if (value is not Havok.hk2019.hkHashMap<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBiplanarInput, Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBiplanarOutput> castValue) return false;
                instance.m_getVerticalClearanceSegmentsBiplanarRecords = castValue;
                return true;
            }
            case "m_getVerticalClearanceSegmentsBasisQuadriplanarRecords":
            case "getVerticalClearanceSegmentsBasisQuadriplanarRecords":
            {
                if (value is not Havok.hk2019.hkHashMap<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBasisQuadriplanarInput, Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBasisQuadriplanarOutput> castValue) return false;
                instance.m_getVerticalClearanceSegmentsBasisQuadriplanarRecords = castValue;
                return true;
            }
            case "m_getGeometryVolumesRecords":
            case "getGeometryVolumesRecords":
            {
                if (value is not Havok.hk2019.hkHashMap<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumesInput, Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumesOutput> castValue) return false;
                instance.m_getGeometryVolumesRecords = castValue;
                return true;
            }
            case "m_getGeometryVolumeClippingSegmentsBiplanarRecords":
            case "getGeometryVolumeClippingSegmentsBiplanarRecords":
            {
                if (value is not Havok.hk2019.hkHashMap<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBiplanarInput, Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBiplanarOutput> castValue) return false;
                instance.m_getGeometryVolumeClippingSegmentsBiplanarRecords = castValue;
                return true;
            }
            case "m_getGeometryVolumeClippingSegmentsBasisQuadriplanarRecords":
            case "getGeometryVolumeClippingSegmentsBasisQuadriplanarRecords":
            {
                if (value is not Havok.hk2019.hkHashMap<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBasisQuadriplanarInput, Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBasisQuadriplanarOutput> castValue) return false;
                instance.m_getGeometryVolumeClippingSegmentsBasisQuadriplanarRecords = castValue;
                return true;
            }
            case "m_geometryFacesIntersectXyPrismRecords":
            case "geometryFacesIntersectXyPrismRecords":
            {
                if (value is not Havok.hk2019.hkHashMap<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryFacesIntersectXyPrismInput, Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryFacesIntersectXyPrismOutput> castValue) return false;
                instance.m_geometryFacesIntersectXyPrismRecords = castValue;
                return true;
            }
            case "m_geometryVolumeIntersectsPointRecords":
            case "geometryVolumeIntersectsPointRecords":
            {
                if (value is not Havok.hk2019.hkHashMap<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryVolumeIntersectsPointInput, Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryVolumeIntersectsPointOutput> castValue) return false;
                instance.m_geometryVolumeIntersectsPointRecords = castValue;
                return true;
            }
            case "m_getTrimeshOutput":
            case "getTrimeshOutput":
            {
                if (value is not Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetTrimeshOutput castValue) return false;
                instance.m_getTrimeshOutput = castValue;
                return true;
            }
            case "m_getTrimeshTrianglesForGeometryFaceRecords":
            case "getTrimeshTrianglesForGeometryFaceRecords":
            {
                if (value is not Havok.hk2019.hkHashMap<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetTrimeshTrianglesForGeometryFaceInput, Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetTrimeshTrianglesForGeometryFaceOutput> castValue) return false;
                instance.m_getTrimeshTrianglesForGeometryFaceRecords = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
