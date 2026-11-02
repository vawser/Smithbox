// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingProcessGeometryFaceOutputAddGeometryVolumeData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput.AddGeometryVolume> 
{
    public hkaiNavMeshGenerationSnapshottingProcessGeometryFaceOutputAddGeometryVolumeData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceOutput.AddGeometryVolume instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometryVolumeKey":
            case "geometryVolumeKey":
            {
                if (instance.m_geometryVolumeKey is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_extrusion":
            case "extrusion":
            {
                if (instance.m_extrusion is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_geometryVolumeData":
            case "geometryVolumeData":
            {
                if (instance.m_geometryVolumeData is not TGet castValue) return false;
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
            case "m_geometryVolumeKey":
            case "geometryVolumeKey":
            {
                if (value is not HKLib.hk2019.hkHandle<uint> castValue) return false;
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
                if (value is not HKLib.hk2019.hkHandle<uint> castValue) return false;
                instance.m_geometryVolumeData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
