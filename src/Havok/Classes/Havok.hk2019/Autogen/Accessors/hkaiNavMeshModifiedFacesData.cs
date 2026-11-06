// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshModifiedFacesData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshModifiedFaces>
{
    public hkaiNavMeshModifiedFacesData(Havok.hk2019.hkaiNavMeshModifiedFaces instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_modifiedFaceIndices":
            case "modifiedFaceIndices":
            {
                value = instance.m_modifiedFaceIndices;
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
            case "m_modifiedFaceIndices":
            case "modifiedFaceIndices":
            {
                if (value is not List<Havok.hk2019.hkHashSet<int>> castValue) return false;
                instance.m_modifiedFaceIndices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
