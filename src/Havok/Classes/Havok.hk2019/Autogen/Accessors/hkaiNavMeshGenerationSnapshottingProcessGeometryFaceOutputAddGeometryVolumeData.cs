// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingProcessGeometryFaceOutputAddGeometryVolumeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput.AddGeometryVolume>
{
    public hkaiNavMeshGenerationSnapshottingProcessGeometryFaceOutputAddGeometryVolumeData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput.AddGeometryVolume instance) : base(instance) { }

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
            case "m_extrusion":
            case "extrusion":
            {
                value = instance.m_extrusion;
                return true;
            }
            case "m_geometryVolumeData":
            case "geometryVolumeData":
            {
                value = instance.m_geometryVolumeData;
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
            case "m_extrusion":
            case "extrusion":
            {
                if (value is not float castValue) return false;
                instance.m_extrusion = castValue;
                return true;
            }
            case "m_geometryVolumeData":
            case "geometryVolumeData":
            {
                if (value is not Havok.hk2019.hkHandle<uint> castValue) return false;
                instance.m_geometryVolumeData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
