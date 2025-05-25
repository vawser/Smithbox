// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiNavMeshClearanceCacheSeeding;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshData : HavokData<hkaiNavMesh> 
{
    public hkaiNavMeshData(HavokType type, hkaiNavMesh instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faces":
            case "faces":
            {
                if (instance.m_faces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (instance.m_edges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertices":
            case "vertices":
            {
                if (instance.m_vertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_streamingSets":
            case "streamingSets":
            {
                if (instance.m_streamingSets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceData":
            case "faceData":
            {
                if (instance.m_faceData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeData":
            case "edgeData":
            {
                if (instance.m_edgeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceDataStriding":
            case "faceDataStriding":
            {
                if (instance.m_faceDataStriding is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeDataStriding":
            case "edgeDataStriding":
            {
                if (instance.m_edgeDataStriding is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (instance.m_aabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_erosionRadius":
            case "erosionRadius":
            {
                if (instance.m_erosionRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clearanceCacheSeedingDataSet":
            case "clearanceCacheSeedingDataSet":
            {
                if (instance.m_clearanceCacheSeedingDataSet is null)
                {
                    return true;
                }
                if (instance.m_clearanceCacheSeedingDataSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_faces":
            case "faces":
            {
                if (value is not List<hkaiNavMesh.Face> castValue) return false;
                instance.m_faces = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (value is not List<hkaiNavMesh.Edge> castValue) return false;
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
                if (value is not List<hkaiAnnotatedStreamingSet> castValue) return false;
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
                if (value is not hkAabb castValue) return false;
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
                if (value is CacheDataSet castValue)
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
