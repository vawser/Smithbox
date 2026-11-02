// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSimpleGeometrySourceThreeDVolumeData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.SimpleGeometrySource.ThreeDVolume> 
{
    public hkaiNavMeshGenerationSimpleGeometrySourceThreeDVolumeData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.SimpleGeometrySource.ThreeDVolume instance) : base(type, instance) {}

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
            case "m_material":
            case "material":
            {
                if (instance.m_material is not TGet castValue) return false;
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
            case "m_material":
            case "material":
            {
                if (value is not uint castValue) return false;
                instance.m_material = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
