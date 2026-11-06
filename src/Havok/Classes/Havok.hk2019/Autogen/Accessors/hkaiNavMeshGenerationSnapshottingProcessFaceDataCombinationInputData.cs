// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingProcessFaceDataCombinationInputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessFaceDataCombinationInput>
{
    public hkaiNavMeshGenerationSnapshottingProcessFaceDataCombinationInputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessFaceDataCombinationInput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceDatas":
            case "faceDatas":
            {
                value = instance.m_faceDatas;
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
