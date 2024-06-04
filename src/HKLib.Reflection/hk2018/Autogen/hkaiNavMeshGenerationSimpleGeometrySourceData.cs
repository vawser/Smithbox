// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiNavMeshGeneration;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshGenerationSimpleGeometrySourceData : HavokData<SimpleGeometrySource> 
{
    public hkaiNavMeshGenerationSimpleGeometrySourceData(HavokType type, SimpleGeometrySource instance) : base(type, instance) {}

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
            case "m_faceUserdataStriding":
            case "faceUserdataStriding":
            {
                if (instance.m_faceUserdataStriding is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_paintingVolumes":
            case "paintingVolumes":
            {
                if (instance.m_paintingVolumes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_carvingVolumes":
            case "carvingVolumes":
            {
                if (instance.m_carvingVolumes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inverseCarvingVolume":
            case "inverseCarvingVolume":
            {
                if (instance.m_inverseCarvingVolume is null)
                {
                    return true;
                }
                if (instance.m_inverseCarvingVolume is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_paintingVolumeUserdata":
            case "paintingVolumeUserdata":
            {
                if (instance.m_paintingVolumeUserdata is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_carvingAreas":
            case "carvingAreas":
            {
                if (instance.m_carvingAreas is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
                if (value is hkGeometry castValue)
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
                if (value is not List<hkGeometry?> castValue) return false;
                instance.m_paintingVolumes = castValue;
                return true;
            }
            case "m_carvingVolumes":
            case "carvingVolumes":
            {
                if (value is not List<hkGeometry?> castValue) return false;
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
                if (value is hkGeometry castValue)
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
