// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGetGeometryFaceMaterialOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFaceMaterialOutput>
{
    public hkaiNavMeshGenerationSnapshottingGetGeometryFaceMaterialOutputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFaceMaterialOutput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometryFaceMaterial":
            case "geometryFaceMaterial":
            {
                value = instance.m_geometryFaceMaterial;
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
            case "m_geometryFaceMaterial":
            case "geometryFaceMaterial":
            {
                if (value is not Havok.hk2019.hkHandle<uint> castValue) return false;
                instance.m_geometryFaceMaterial = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
