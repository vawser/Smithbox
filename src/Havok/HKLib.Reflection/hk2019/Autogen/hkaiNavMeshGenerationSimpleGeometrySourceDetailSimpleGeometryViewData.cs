// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSimpleGeometrySourceDetailSimpleGeometryViewData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.SimpleGeometryView> 
{
    public hkaiNavMeshGenerationSimpleGeometrySourceDetailSimpleGeometryViewData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.SimpleGeometryView instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_source":
            case "source":
            {
                if (instance.m_source is null)
                {
                    return true;
                }
                if (instance.m_source is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertices":
            case "vertices":
            {
                if (instance.m_vertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_potentiallyWalkableFaceKeys":
            case "potentiallyWalkableFaceKeys":
            {
                if (instance.m_potentiallyWalkableFaceKeys is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_trianglesTree":
            case "trianglesTree":
            {
                if (instance.m_trianglesTree is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_threeDVolumes":
            case "threeDVolumes":
            {
                if (instance.m_threeDVolumes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_threeDVolumesTree":
            case "threeDVolumesTree":
            {
                if (instance.m_threeDVolumesTree is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_areaVolumes":
            case "areaVolumes":
            {
                if (instance.m_areaVolumes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_areaVolumesTree":
            case "areaVolumesTree":
            {
                if (instance.m_areaVolumesTree is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_domainToWorld":
            case "domainToWorld":
            {
                if (instance.m_domainToWorld is not TGet castValue) return false;
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
            case "m_source":
            case "source":
            {
                if (value is null)
                {
                    instance.m_source = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiNavMeshGeneration.SimpleGeometrySource castValue)
                {
                    instance.m_source = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertices":
            case "vertices":
            {
                if (value is not List<HKLib.hk2019.hkFpMath.FpIntVec3_4> castValue) return false;
                instance.m_vertices = castValue;
                return true;
            }
            case "m_potentiallyWalkableFaceKeys":
            case "potentiallyWalkableFaceKeys":
            {
                if (value is not List<HKLib.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.GeometryFaceKey> castValue) return false;
                instance.m_potentiallyWalkableFaceKeys = castValue;
                return true;
            }
            case "m_trianglesTree":
            case "trianglesTree":
            {
                if (value is not HKLib.hk2019.hkcdSimdTree castValue) return false;
                instance.m_trianglesTree = castValue;
                return true;
            }
            case "m_threeDVolumes":
            case "threeDVolumes":
            {
                if (value is not List<HKLib.hk2019.hkaiNavMeshGeneration.InputVolume?> castValue) return false;
                instance.m_threeDVolumes = castValue;
                return true;
            }
            case "m_threeDVolumesTree":
            case "threeDVolumesTree":
            {
                if (value is not HKLib.hk2019.hkcdSimdTree castValue) return false;
                instance.m_threeDVolumesTree = castValue;
                return true;
            }
            case "m_areaVolumes":
            case "areaVolumes":
            {
                if (value is not List<HKLib.hk2019.hkaiNavMeshGeneration.InputArea?> castValue) return false;
                instance.m_areaVolumes = castValue;
                return true;
            }
            case "m_areaVolumesTree":
            case "areaVolumesTree":
            {
                if (value is not HKLib.hk2019.hkcdSimdTree castValue) return false;
                instance.m_areaVolumesTree = castValue;
                return true;
            }
            case "m_domainToWorld":
            case "domainToWorld":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_domainToWorld = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
