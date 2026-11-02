// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingProcessGeometryFaceOutputAddVerticalClearanceTestData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput.AddVerticalClearanceTest> 
{
    public hkaiNavMeshGenerationSnapshottingProcessGeometryFaceOutputAddVerticalClearanceTestData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput.AddVerticalClearanceTest instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_height":
            case "height":
            {
                if (instance.m_height is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (instance.m_data is not TGet castValue) return false;
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
            case "m_height":
            case "height":
            {
                if (value is not float castValue) return false;
                instance.m_height = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (value is not HKLib.hk2019.hkHandle<uint> castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
