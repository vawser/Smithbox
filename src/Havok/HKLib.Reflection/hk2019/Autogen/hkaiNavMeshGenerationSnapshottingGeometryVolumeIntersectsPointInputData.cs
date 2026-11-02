// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingGeometryVolumeIntersectsPointInputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryVolumeIntersectsPointInput> 
{
    public hkaiNavMeshGenerationSnapshottingGeometryVolumeIntersectsPointInputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryVolumeIntersectsPointInput instance) : base(type, instance) {}

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
            case "m_point":
            case "point":
            {
                if (instance.m_point is not TGet castValue) return false;
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
            case "m_point":
            case "point":
            {
                if (value is not HKLib.hk2019.hkFpMath.FpFusedRationalVec3 castValue) return false;
                instance.m_point = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
