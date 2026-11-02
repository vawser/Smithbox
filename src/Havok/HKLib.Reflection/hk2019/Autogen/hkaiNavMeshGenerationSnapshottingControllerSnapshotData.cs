// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingControllerSnapshotData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ControllerSnapshot> 
{
    public hkaiNavMeshGenerationSnapshottingControllerSnapshotData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ControllerSnapshot instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_processGeometryFaceRecords":
            case "processGeometryFaceRecords":
            {
                if (instance.m_processGeometryFaceRecords is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_processGeometryFragmentRecords":
            case "processGeometryFragmentRecords":
            {
                if (instance.m_processGeometryFragmentRecords is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shouldVolumeBlockConnectionRecords":
            case "shouldVolumeBlockConnectionRecords":
            {
                if (instance.m_shouldVolumeBlockConnectionRecords is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_processFaceDataCombinationRecords":
            case "processFaceDataCombinationRecords":
            {
                if (instance.m_processFaceDataCombinationRecords is not TGet castValue) return false;
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
            case "m_processGeometryFaceRecords":
            case "processGeometryFaceRecords":
            {
                if (value is not HKLib.hk2019.hkHashMap<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceInput, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput> castValue) return false;
                instance.m_processGeometryFaceRecords = castValue;
                return true;
            }
            case "m_processGeometryFragmentRecords":
            case "processGeometryFragmentRecords":
            {
                if (value is not HKLib.hk2019.hkHashMap<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFragmentInput, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFragmentOutput> castValue) return false;
                instance.m_processGeometryFragmentRecords = castValue;
                return true;
            }
            case "m_shouldVolumeBlockConnectionRecords":
            case "shouldVolumeBlockConnectionRecords":
            {
                if (value is not HKLib.hk2019.hkHashMap<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ShouldVolumeBlockConnectionInput, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ShouldVolumeBlockConnectionOutput> castValue) return false;
                instance.m_shouldVolumeBlockConnectionRecords = castValue;
                return true;
            }
            case "m_processFaceDataCombinationRecords":
            case "processFaceDataCombinationRecords":
            {
                if (value is not HKLib.hk2019.hkHashMap<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessFaceDataCombinationInput, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessFaceDataCombinationOutput> castValue) return false;
                instance.m_processFaceDataCombinationRecords = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
