// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGetGeometryVolumesInputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumesInput>
{
    public hkaiNavMeshGenerationSnapshottingGetGeometryVolumesInputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumesInput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aabb":
            case "aabb":
            {
                value = instance.m_aabb;
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
            case "m_aabb":
            case "aabb":
            {
                if (value is not Havok.hk2019.hkAabb castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
