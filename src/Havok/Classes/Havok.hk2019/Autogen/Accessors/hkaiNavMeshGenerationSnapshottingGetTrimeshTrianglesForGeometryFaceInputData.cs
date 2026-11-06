// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGetTrimeshTrianglesForGeometryFaceInputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetTrimeshTrianglesForGeometryFaceInput>
{
    public hkaiNavMeshGenerationSnapshottingGetTrimeshTrianglesForGeometryFaceInputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetTrimeshTrianglesForGeometryFaceInput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometryFaceKey":
            case "geometryFaceKey":
            {
                value = instance.m_geometryFaceKey;
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
            case "m_geometryFaceKey":
            case "geometryFaceKey":
            {
                if (value is not Havok.hk2019.hkHandle<uint> castValue) return false;
                instance.m_geometryFaceKey = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
