// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshQueryMediatorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshQueryMediator>
{
    public hkaiNavMeshQueryMediatorData(Havok.hk2019.hkaiNavMeshQueryMediator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceIterator":
            case "faceIterator":
            {
                value = instance.m_faceIterator;
                return true;
            }
            case "m_mesh":
            case "mesh":
            {
                value = instance.m_mesh;
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
            case "m_faceIterator":
            case "faceIterator":
            {
                if (value is null)
                {
                    instance.m_faceIterator = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiNavMeshFaceIterator castValue)
                {
                    instance.m_faceIterator = castValue;
                    return true;
                }
                return false;
            }
            case "m_mesh":
            case "mesh":
            {
                if (value is null)
                {
                    instance.m_mesh = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiNavMesh castValue)
                {
                    instance.m_mesh = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
