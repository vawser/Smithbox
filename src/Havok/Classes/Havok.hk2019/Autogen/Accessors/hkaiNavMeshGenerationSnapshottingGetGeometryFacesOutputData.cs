// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGetGeometryFacesOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFacesOutput>
{
    public hkaiNavMeshGenerationSnapshottingGetGeometryFacesOutputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFacesOutput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometryFaceKeys":
            case "geometryFaceKeys":
            {
                value = instance.m_geometryFaceKeys;
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
            case "m_geometryFaceKeys":
            case "geometryFaceKeys":
            {
                if (value is not List<Havok.hk2019.hkHandle<uint>> castValue) return false;
                instance.m_geometryFaceKeys = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
