// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSimpleGeometrySourceData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.SimpleGeometrySource> 
{
    public hkaiNavMeshGenerationSimpleGeometrySourceData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.SimpleGeometrySource instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometry":
            case "geometry":
            {
                if (instance.m_geometry is null)
                {
                    return true;
                }
                if (instance.m_geometry is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_threeDVolumes":
            case "threeDVolumes":
            {
                if (instance.m_threeDVolumes is not TGet castValue) return false;
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
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is HKLib.hk2019.hkGeometry castValue)
                {
                    instance.m_geometry = castValue;
                    return true;
                }
                return false;
            }
            case "m_threeDVolumes":
            case "threeDVolumes":
            {
                if (value is not List<HKLib.hk2019.hkaiNavMeshGeneration.SimpleGeometrySource.ThreeDVolume> castValue) return false;
                instance.m_threeDVolumes = castValue;
                return true;
            }
            case "m_areaVolumes":
            case "areaVolumes":
            {
                if (value is not List<HKLib.hk2019.hkaiNavMeshGeneration.SimpleGeometrySource.AreaVolume> castValue) return false;
                instance.m_areaVolumes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
