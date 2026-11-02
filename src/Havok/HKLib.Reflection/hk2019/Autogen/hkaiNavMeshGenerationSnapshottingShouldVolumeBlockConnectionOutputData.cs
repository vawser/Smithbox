// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingShouldVolumeBlockConnectionOutputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ShouldVolumeBlockConnectionOutput> 
{
    public hkaiNavMeshGenerationSnapshottingShouldVolumeBlockConnectionOutputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ShouldVolumeBlockConnectionOutput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_result":
            case "result":
            {
                if (instance.m_result is not TGet castValue) return false;
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
            case "m_result":
            case "result":
            {
                if (value is not bool castValue) return false;
                instance.m_result = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
