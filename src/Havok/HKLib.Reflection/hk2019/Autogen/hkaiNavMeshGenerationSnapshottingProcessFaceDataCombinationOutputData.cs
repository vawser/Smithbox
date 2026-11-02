// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingProcessFaceDataCombinationOutputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessFaceDataCombinationOutput> 
{
    public hkaiNavMeshGenerationSnapshottingProcessFaceDataCombinationOutputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessFaceDataCombinationOutput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceData":
            case "faceData":
            {
                if (instance.m_faceData is not TGet castValue) return false;
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
            case "m_faceData":
            case "faceData":
            {
                if (value is not List<int> castValue) return false;
                instance.m_faceData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
