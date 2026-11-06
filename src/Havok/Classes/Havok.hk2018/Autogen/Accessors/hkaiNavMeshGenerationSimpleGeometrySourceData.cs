// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshGenerationSimpleGeometrySourceData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshGeneration.SimpleGeometrySource>
{
    public hkaiNavMeshGenerationSimpleGeometrySourceData(Havok.hk2018.hkaiNavMeshGeneration.SimpleGeometrySource instance) : base(instance) { }

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
            case "m_geometry":
            case "geometry":
            {
                value = instance.m_geometry;
                return true;
            }
            case "m_faceUserdataStriding":
            case "faceUserdataStriding":
            {
                value = instance.m_faceUserdataStriding;
                return true;
            }
            case "m_paintingVolumes":
            case "paintingVolumes":
            {
                value = instance.m_paintingVolumes;
                return true;
            }
            case "m_carvingVolumes":
            case "carvingVolumes":
            {
                value = instance.m_carvingVolumes;
                return true;
            }
            case "m_inverseCarvingVolume":
            case "inverseCarvingVolume":
            {
                value = instance.m_inverseCarvingVolume;
                return true;
            }
            case "m_paintingVolumeUserdata":
            case "paintingVolumeUserdata":
            {
                value = instance.m_paintingVolumeUserdata;
                return true;
            }
            case "m_carvingAreas":
            case "carvingAreas":
            {
                value = instance.m_carvingAreas;
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
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_geometry":
            case "geometry":
            {
                if (value is null)
                {
                    instance.m_geometry = default;
                    return true;
                }
                if (value is Havok.hk2018.hkGeometry castValue)
                {
                    instance.m_geometry = castValue;
                    return true;
                }
                return false;
            }
            case "m_faceUserdataStriding":
            case "faceUserdataStriding":
            {
                if (value is not int castValue) return false;
                instance.m_faceUserdataStriding = castValue;
                return true;
            }
            case "m_paintingVolumes":
            case "paintingVolumes":
            {
                if (value is not List<Havok.hk2018.hkGeometry?> castValue) return false;
                instance.m_paintingVolumes = castValue;
                return true;
            }
            case "m_carvingVolumes":
            case "carvingVolumes":
            {
                if (value is not List<Havok.hk2018.hkGeometry?> castValue) return false;
                instance.m_carvingVolumes = castValue;
                return true;
            }
            case "m_inverseCarvingVolume":
            case "inverseCarvingVolume":
            {
                if (value is null)
                {
                    instance.m_inverseCarvingVolume = default;
                    return true;
                }
                if (value is Havok.hk2018.hkGeometry castValue)
                {
                    instance.m_inverseCarvingVolume = castValue;
                    return true;
                }
                return false;
            }
            case "m_paintingVolumeUserdata":
            case "paintingVolumeUserdata":
            {
                if (value is not List<int> castValue) return false;
                instance.m_paintingVolumeUserdata = castValue;
                return true;
            }
            case "m_carvingAreas":
            case "carvingAreas":
            {
                if (value is not List<List<Vector4>> castValue) return false;
                instance.m_carvingAreas = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
