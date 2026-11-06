// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGeometryVolumeIntersectsPointInputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryVolumeIntersectsPointInput>
{
    public hkaiNavMeshGenerationSnapshottingGeometryVolumeIntersectsPointInputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryVolumeIntersectsPointInput instance) : base(instance) { }

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
            case "m_point":
            case "point":
            {
                value = instance.m_point;
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
            case "m_point":
            case "point":
            {
                if (value is not Havok.hk2019.hkFpMath.FpFusedRationalVec3 castValue) return false;
                instance.m_point = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
