// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingProcessGeometryFaceOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput>
{
    public hkaiNavMeshGenerationSnapshottingProcessGeometryFaceOutputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_clipFace":
            case "clipFace":
            {
                value = instance.m_clipFace;
                return true;
            }
            case "m_geometryFaceData":
            case "geometryFaceData":
            {
                value = instance.m_geometryFaceData;
                return true;
            }
            case "m_addVerticalClearanceTests":
            case "addVerticalClearanceTests":
            {
                value = instance.m_addVerticalClearanceTests;
                return true;
            }
            case "m_addGeometryVolumes":
            case "addGeometryVolumes":
            {
                value = instance.m_addGeometryVolumes;
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
            case "m_clipFace":
            case "clipFace":
            {
                if (value is not bool castValue) return false;
                instance.m_clipFace = castValue;
                return true;
            }
            case "m_geometryFaceData":
            case "geometryFaceData":
            {
                if (value is not Havok.hk2019.hkHandle<uint> castValue) return false;
                instance.m_geometryFaceData = castValue;
                return true;
            }
            case "m_addVerticalClearanceTests":
            case "addVerticalClearanceTests":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput.AddVerticalClearanceTest> castValue) return false;
                instance.m_addVerticalClearanceTests = castValue;
                return true;
            }
            case "m_addGeometryVolumes":
            case "addGeometryVolumes":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput.AddGeometryVolume> castValue) return false;
                instance.m_addGeometryVolumes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
