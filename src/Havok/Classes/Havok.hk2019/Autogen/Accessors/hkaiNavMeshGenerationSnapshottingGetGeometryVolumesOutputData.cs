// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGetGeometryVolumesOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumesOutput>
{
    public hkaiNavMeshGenerationSnapshottingGetGeometryVolumesOutputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumesOutput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_volumes":
            case "volumes":
            {
                value = instance.m_volumes;
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
            case "m_volumes":
            case "volumes":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumesOutput.Volume> castValue) return false;
                instance.m_volumes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
