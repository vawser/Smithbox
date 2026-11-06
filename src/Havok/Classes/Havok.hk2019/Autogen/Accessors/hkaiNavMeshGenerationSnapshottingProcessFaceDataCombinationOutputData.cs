// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingProcessFaceDataCombinationOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessFaceDataCombinationOutput>
{
    public hkaiNavMeshGenerationSnapshottingProcessFaceDataCombinationOutputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessFaceDataCombinationOutput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceData":
            case "faceData":
            {
                value = instance.m_faceData;
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
