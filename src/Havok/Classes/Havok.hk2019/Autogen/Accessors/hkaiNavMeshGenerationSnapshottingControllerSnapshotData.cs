// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingControllerSnapshotData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ControllerSnapshot>
{
    public hkaiNavMeshGenerationSnapshottingControllerSnapshotData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ControllerSnapshot instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_processGeometryFaceRecords":
            case "processGeometryFaceRecords":
            {
                value = instance.m_processGeometryFaceRecords;
                return true;
            }
            case "m_processGeometryFragmentRecords":
            case "processGeometryFragmentRecords":
            {
                value = instance.m_processGeometryFragmentRecords;
                return true;
            }
            case "m_shouldVolumeBlockConnectionRecords":
            case "shouldVolumeBlockConnectionRecords":
            {
                value = instance.m_shouldVolumeBlockConnectionRecords;
                return true;
            }
            case "m_processFaceDataCombinationRecords":
            case "processFaceDataCombinationRecords":
            {
                value = instance.m_processFaceDataCombinationRecords;
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
            case "m_processGeometryFaceRecords":
            case "processGeometryFaceRecords":
            {
                if (value is not Havok.hk2019.hkHashMap<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceInput, Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput> castValue) return false;
                instance.m_processGeometryFaceRecords = castValue;
                return true;
            }
            case "m_processGeometryFragmentRecords":
            case "processGeometryFragmentRecords":
            {
                if (value is not Havok.hk2019.hkHashMap<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFragmentInput, Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFragmentOutput> castValue) return false;
                instance.m_processGeometryFragmentRecords = castValue;
                return true;
            }
            case "m_shouldVolumeBlockConnectionRecords":
            case "shouldVolumeBlockConnectionRecords":
            {
                if (value is not Havok.hk2019.hkHashMap<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ShouldVolumeBlockConnectionInput, Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ShouldVolumeBlockConnectionOutput> castValue) return false;
                instance.m_shouldVolumeBlockConnectionRecords = castValue;
                return true;
            }
            case "m_processFaceDataCombinationRecords":
            case "processFaceDataCombinationRecords":
            {
                if (value is not Havok.hk2019.hkHashMap<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessFaceDataCombinationInput, Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessFaceDataCombinationOutput> castValue) return false;
                instance.m_processFaceDataCombinationRecords = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
