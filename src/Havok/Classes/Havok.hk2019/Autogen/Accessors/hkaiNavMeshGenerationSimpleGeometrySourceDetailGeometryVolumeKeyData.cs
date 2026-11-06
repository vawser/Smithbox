// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSimpleGeometrySourceDetailGeometryVolumeKeyData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.GeometryVolumeKey>
{
    public hkaiNavMeshGenerationSimpleGeometrySourceDetailGeometryVolumeKeyData(Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.GeometryVolumeKey instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_index":
            case "index":
            {
                value = instance.m_index;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.VolumeType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_type = (Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.VolumeType)intValue;
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
