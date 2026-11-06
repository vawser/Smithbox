// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSimpleGeometrySourceAreaVolumeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySource.AreaVolume>
{
    public hkaiNavMeshGenerationSimpleGeometrySourceAreaVolumeData(Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySource.AreaVolume instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertices":
            case "vertices":
            {
                value = instance.m_vertices;
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
            case "m_vertices":
            case "vertices":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_vertices = castValue;
                return true;
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
