// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingGetGeometryVolumesOutputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumesOutput> 
{
    public hkaiNavMeshGenerationSnapshottingGetGeometryVolumesOutputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumesOutput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_volumes":
            case "volumes":
            {
                if (instance.m_volumes is not TGet castValue) return false;
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
            case "m_volumes":
            case "volumes":
            {
                if (value is not List<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumesOutput.Volume> castValue) return false;
                instance.m_volumes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
