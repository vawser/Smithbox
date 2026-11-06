// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGetGeometryVolumesOutputVolumeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumesOutput.Volume>
{
    public hkaiNavMeshGenerationSnapshottingGetGeometryVolumesOutputVolumeData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumesOutput.Volume instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometryVolumeKey":
            case "geometryVolumeKey":
            {
                value = instance.m_geometryVolumeKey;
                return true;
            }
            case "m_geometryVolumeMaterial":
            case "geometryVolumeMaterial":
            {
                value = instance.m_geometryVolumeMaterial;
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
            case "m_geometryVolumeKey":
            case "geometryVolumeKey":
            {
                if (value is not Havok.hk2019.hkHandle<uint> castValue) return false;
                instance.m_geometryVolumeKey = castValue;
                return true;
            }
            case "m_geometryVolumeMaterial":
            case "geometryVolumeMaterial":
            {
                if (value is not Havok.hk2019.hkHandle<uint> castValue) return false;
                instance.m_geometryVolumeMaterial = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
