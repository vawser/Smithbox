// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkaiNavMeshData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkaiNavMesh>
{
    public hkaiNavMeshData(Havok.hk2016.hkaiNavMesh instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_clearanceCacheSeedingDataSet":
            case "clearanceCacheSeedingDataSet":
            {
                value = instance.m_clearanceCacheSeedingDataSet;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_faces":
            case "faces":
            {
                if (value is not List<Havok.hk2016.hkaiNavMesh.Face> castValue) return false;
                instance.m_faces = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (value is not List<Havok.hk2016.hkaiNavMesh.Edge> castValue) return false;
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
                if (value is not List<Havok.hk2016.hkaiAnnotatedStreamingSet> castValue) return false;
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
                if (value is not Havok.hk2016.hkAabb castValue) return false;
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
            case "m_clearanceCacheSeedingDataSet":
            case "clearanceCacheSeedingDataSet":
            {
                if (value is null)
                {
                    instance.m_clearanceCacheSeedingDataSet = default;
                    return true;
                }
                if (value is Havok.hk2016.hkaiNavMeshClearanceCacheSeeding.CacheDataSet castValue)
                {
                    instance.m_clearanceCacheSeedingDataSet = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
