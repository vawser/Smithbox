// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGetTrimeshTrianglesForGeometryFaceOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetTrimeshTrianglesForGeometryFaceOutput>
{
    public hkaiNavMeshGenerationSnapshottingGetTrimeshTrianglesForGeometryFaceOutputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetTrimeshTrianglesForGeometryFaceOutput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_indices":
            case "indices":
            {
                value = instance.m_indices;
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
            case "m_indices":
            case "indices":
            {
                if (value is not List<int> castValue) return false;
                instance.m_indices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
