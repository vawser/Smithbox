// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGetTrimeshOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetTrimeshOutput>
{
    public hkaiNavMeshGenerationSnapshottingGetTrimeshOutputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetTrimeshOutput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_trimesh":
            case "trimesh":
            {
                value = instance.m_trimesh;
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
            case "m_trimesh":
            case "trimesh":
            {
                if (value is null)
                {
                    instance.m_trimesh = default;
                    return true;
                }
                if (value is Havok.hk2019.hkGeometry castValue)
                {
                    instance.m_trimesh = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
