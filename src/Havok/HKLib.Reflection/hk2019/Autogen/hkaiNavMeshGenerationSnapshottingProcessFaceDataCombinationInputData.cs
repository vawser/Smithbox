// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingProcessFaceDataCombinationInputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessFaceDataCombinationInput> 
{
    public hkaiNavMeshGenerationSnapshottingProcessFaceDataCombinationInputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessFaceDataCombinationInput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceDatas":
            case "faceDatas":
            {
                if (instance.m_faceDatas is not TGet castValue) return false;
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
            case "m_faceDatas":
            case "faceDatas":
            {
                if (value is not List<int> castValue) return false;
                instance.m_faceDatas = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
