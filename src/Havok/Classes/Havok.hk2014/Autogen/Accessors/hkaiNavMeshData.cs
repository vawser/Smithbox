// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiNavMeshData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiNavMesh>
{
    public hkaiNavMeshData(Havok.hk2014.hkaiNavMesh instance) : base(instance) { }

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
            case "m_streamingSets":
            case "streamingSets":
            {
                value = instance.m_streamingSets;
                return true;
            }
            case "m_faceData":
            case "faceData":
            {
                value = instance.m_faceData;
                return true;
            }
            case "m_edgeData":
            case "edgeData":
            {
                value = instance.m_edgeData;
                return true;
            }
            case "m_faceDataStriding":
            case "faceDataStriding":
            {
                value = instance.m_faceDataStriding;
                return true;
            }
            case "m_edgeDataStriding":
            case "edgeDataStriding":
            {
                value = instance.m_edgeDataStriding;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                value = instance.m_aabb;
                return true;
            }
            case "m_erosionRadius":
            case "erosionRadius":
            {
                value = instance.m_erosionRadius;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
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
                if (value is not List<Havok.hk2014.hkaiNavMesh.Face> castValue) return false;
                instance.m_faces = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (value is not List<Havok.hk2014.hkaiNavMesh.Edge> castValue) return false;
                instance.m_edges = castValue;
                return true;
            }
            case "m_vertices":
            case "vertices":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_vertices = castValue;
                return true;
            }
            case "m_streamingSets":
            case "streamingSets":
            {
                if (value is not List<Havok.hk2014.hkaiStreamingSet> castValue) return false;
                instance.m_streamingSets = castValue;
                return true;
            }
            case "m_faceData":
            case "faceData":
            {
                if (value is not List<int> castValue) return false;
                instance.m_faceData = castValue;
                return true;
            }
            case "m_edgeData":
            case "edgeData":
            {
                if (value is not List<int> castValue) return false;
                instance.m_edgeData = castValue;
                return true;
            }
            case "m_faceDataStriding":
            case "faceDataStriding":
            {
                if (value is not int castValue) return false;
                instance.m_faceDataStriding = castValue;
                return true;
            }
            case "m_edgeDataStriding":
            case "edgeDataStriding":
            {
                if (value is not int castValue) return false;
                instance.m_edgeDataStriding = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not byte castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (value is not Havok.hk2014.hkAabb castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            case "m_erosionRadius":
            case "erosionRadius":
            {
                if (value is not float castValue) return false;
                instance.m_erosionRadius = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
