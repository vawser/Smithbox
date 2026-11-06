// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingProcessGeometryFaceOutputAddVerticalClearanceTestData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput.AddVerticalClearanceTest>
{
    public hkaiNavMeshGenerationSnapshottingProcessGeometryFaceOutputAddVerticalClearanceTestData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput.AddVerticalClearanceTest instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_height":
            case "height":
            {
                value = instance.m_height;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
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
                if (value is not Havok.hk2019.hkHandle<uint> castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
