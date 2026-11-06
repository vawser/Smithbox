// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSimpleGeometrySourceDetailSimpleGeometryViewData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.SimpleGeometryView>
{
    public hkaiNavMeshGenerationSimpleGeometrySourceDetailSimpleGeometryViewData(Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.SimpleGeometryView instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_source":
            case "source":
            {
                value = instance.m_source;
                return true;
            }
            case "m_vertices":
            case "vertices":
            {
                value = instance.m_vertices;
                return true;
            }
            case "m_potentiallyWalkableFaceKeys":
            case "potentiallyWalkableFaceKeys":
            {
                value = instance.m_potentiallyWalkableFaceKeys;
                return true;
            }
            case "m_trianglesTree":
            case "trianglesTree":
            {
                value = instance.m_trianglesTree;
                return true;
            }
            case "m_threeDVolumes":
            case "threeDVolumes":
            {
                value = instance.m_threeDVolumes;
                return true;
            }
            case "m_threeDVolumesTree":
            case "threeDVolumesTree":
            {
                value = instance.m_threeDVolumesTree;
                return true;
            }
            case "m_areaVolumes":
            case "areaVolumes":
            {
                value = instance.m_areaVolumes;
                return true;
            }
            case "m_areaVolumesTree":
            case "areaVolumesTree":
            {
                value = instance.m_areaVolumesTree;
                return true;
            }
            case "m_domainToWorld":
            case "domainToWorld":
            {
                value = instance.m_domainToWorld;
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
            case "m_source":
            case "source":
            {
                if (value is null)
                {
                    instance.m_source = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySource castValue)
                {
                    instance.m_source = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertices":
            case "vertices":
            {
                if (value is not List<Havok.hk2019.hkFpMath.FpIntVec3_4> castValue) return false;
                instance.m_vertices = castValue;
                return true;
            }
            case "m_potentiallyWalkableFaceKeys":
            case "potentiallyWalkableFaceKeys":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.GeometryFaceKey> castValue) return false;
                instance.m_potentiallyWalkableFaceKeys = castValue;
                return true;
            }
            case "m_trianglesTree":
            case "trianglesTree":
            {
                if (value is not Havok.hk2019.hkcdSimdTree castValue) return false;
                instance.m_trianglesTree = castValue;
                return true;
            }
            case "m_threeDVolumes":
            case "threeDVolumes":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshGeneration.InputVolume?> castValue) return false;
                instance.m_threeDVolumes = castValue;
                return true;
            }
            case "m_threeDVolumesTree":
            case "threeDVolumesTree":
            {
                if (value is not Havok.hk2019.hkcdSimdTree castValue) return false;
                instance.m_threeDVolumesTree = castValue;
                return true;
            }
            case "m_areaVolumes":
            case "areaVolumes":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshGeneration.InputArea?> castValue) return false;
                instance.m_areaVolumes = castValue;
                return true;
            }
            case "m_areaVolumesTree":
            case "areaVolumesTree":
            {
                if (value is not Havok.hk2019.hkcdSimdTree castValue) return false;
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
