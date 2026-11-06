// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSimpleGeometrySourceData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySource>
{
    public hkaiNavMeshGenerationSimpleGeometrySourceData(Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySource instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometry":
            case "geometry":
            {
                value = instance.m_geometry;
                return true;
            }
            case "m_threeDVolumes":
            case "threeDVolumes":
            {
                value = instance.m_threeDVolumes;
                return true;
            }
            case "m_areaVolumes":
            case "areaVolumes":
            {
                value = instance.m_areaVolumes;
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
            case "m_geometry":
            case "geometry":
            {
                if (value is null)
                {
                    instance.m_geometry = default;
                    return true;
                }
                if (value is Havok.hk2019.hkGeometry castValue)
                {
                    instance.m_geometry = castValue;
                    return true;
                }
                return false;
            }
            case "m_threeDVolumes":
            case "threeDVolumes":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySource.ThreeDVolume> castValue) return false;
                instance.m_threeDVolumes = castValue;
                return true;
            }
            case "m_areaVolumes":
            case "areaVolumes":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySource.AreaVolume> castValue) return false;
                instance.m_areaVolumes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
