// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSimpleGeometrySourceDetailGeometryVolumeKeyData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.GeometryVolumeKey> 
{
    public hkaiNavMeshGenerationSimpleGeometrySourceDetailGeometryVolumeKeyData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.GeometryVolumeKey instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_type is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_index":
            case "index":
            {
                if (instance.m_index is not TGet castValue) return false;
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
            case "m_type":
            case "type":
            {
                if (value is HKLib.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.VolumeType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_type = (HKLib.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.VolumeType)intValue;
                    return true;
                }
                return false;
            }
            case "m_index":
            case "index":
            {
                if (value is not int castValue) return false;
                instance.m_index = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
