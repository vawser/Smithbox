// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGeometryVolumeIntersectsPointRecordData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryVolumeIntersectsPointRecord>
{
    public hkaiNavMeshGenerationSnapshottingGeometryVolumeIntersectsPointRecordData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryVolumeIntersectsPointRecord instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_input":
            case "input":
            {
                value = instance.m_input;
                return true;
            }
            case "m_output":
            case "output":
            {
                value = instance.m_output;
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
            case "m_input":
            case "input":
            {
                if (value is not Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryVolumeIntersectsPointInput castValue) return false;
                instance.m_input = castValue;
                return true;
            }
            case "m_output":
            case "output":
            {
                if (value is not Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryVolumeIntersectsPointOutput castValue) return false;
                instance.m_output = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
