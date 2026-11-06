// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSimpleGeometrySourceDetailGeometryFaceKeyData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.GeometryFaceKey>
{
    public hkaiNavMeshGenerationSimpleGeometrySourceDetailGeometryFaceKeyData(Havok.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.GeometryFaceKey instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
