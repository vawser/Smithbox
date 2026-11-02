// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingShouldVolumeBlockConnectionInputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ShouldVolumeBlockConnectionInput> 
{
    public hkaiNavMeshGenerationSnapshottingShouldVolumeBlockConnectionInputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ShouldVolumeBlockConnectionInput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_material":
            case "material":
            {
                if (instance.m_material is not TGet castValue) return false;
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
            case "m_material":
            case "material":
            {
                if (value is not HKLib.hk2019.hkHandle<uint> castValue) return false;
                instance.m_material = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
