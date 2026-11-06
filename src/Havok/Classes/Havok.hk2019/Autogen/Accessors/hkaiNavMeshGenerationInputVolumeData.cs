// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationInputVolumeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.InputVolume>
{
    public hkaiNavMeshGenerationInputVolumeData(Havok.hk2019.hkaiNavMeshGeneration.InputVolume instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faces":
            case "faces":
            {
                value = instance.m_faces;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                value = instance.m_edges;
                return true;
            }
            case "m_vertices":
            case "vertices":
            {
                value = instance.m_vertices;
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
            case "m_faces":
            case "faces":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshGeneration.InputVolume.Face> castValue) return false;
                instance.m_faces = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshGeneration.InputVolume.Edge> castValue) return false;
                instance.m_edges = castValue;
                return true;
            }
            case "m_vertices":
            case "vertices":
            {
                if (value is not List<Havok.hk2019.hkFpMath.FpIntVec3_4> castValue) return false;
                instance.m_vertices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
