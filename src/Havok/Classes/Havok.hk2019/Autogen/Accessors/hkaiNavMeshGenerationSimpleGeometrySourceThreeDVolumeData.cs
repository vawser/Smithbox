// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSimpleGeometrySourceThreeDVolumeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySource.ThreeDVolume>
{
    public hkaiNavMeshGenerationSimpleGeometrySourceThreeDVolumeData(Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySource.ThreeDVolume instance) : base(instance) { }

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
            case "m_material":
            case "material":
            {
                value = instance.m_material;
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
