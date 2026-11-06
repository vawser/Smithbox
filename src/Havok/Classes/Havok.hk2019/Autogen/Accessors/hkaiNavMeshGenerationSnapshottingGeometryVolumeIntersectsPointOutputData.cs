// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGeometryVolumeIntersectsPointOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryVolumeIntersectsPointOutput>
{
    public hkaiNavMeshGenerationSnapshottingGeometryVolumeIntersectsPointOutputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryVolumeIntersectsPointOutput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_result":
            case "result":
            {
                value = instance.m_result;
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
