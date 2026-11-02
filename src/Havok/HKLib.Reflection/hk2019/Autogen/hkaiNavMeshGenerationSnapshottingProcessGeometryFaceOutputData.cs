// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingProcessGeometryFaceOutputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput> 
{
    public hkaiNavMeshGenerationSnapshottingProcessGeometryFaceOutputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_clipFace":
            case "clipFace":
            {
                if (instance.m_clipFace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_geometryFaceData":
            case "geometryFaceData":
            {
                if (instance.m_geometryFaceData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_addVerticalClearanceTests":
            case "addVerticalClearanceTests":
            {
                if (instance.m_addVerticalClearanceTests is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_addGeometryVolumes":
            case "addGeometryVolumes":
            {
                if (instance.m_addGeometryVolumes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is not HKLib.hk2019.hkHandle<uint> castValue) return false;
                instance.m_geometryFaceData = castValue;
                return true;
            }
            case "m_addVerticalClearanceTests":
            case "addVerticalClearanceTests":
            {
                if (value is not List<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput.AddVerticalClearanceTest> castValue) return false;
                instance.m_addVerticalClearanceTests = castValue;
                return true;
            }
            case "m_addGeometryVolumes":
            case "addGeometryVolumes":
            {
                if (value is not List<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput.AddGeometryVolume> castValue) return false;
                instance.m_addGeometryVolumes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
