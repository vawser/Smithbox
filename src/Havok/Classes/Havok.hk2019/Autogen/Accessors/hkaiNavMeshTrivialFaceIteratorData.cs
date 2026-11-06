// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshTrivialFaceIteratorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshTrivialFaceIterator>
{
    public hkaiNavMeshTrivialFaceIteratorData(Havok.hk2019.hkaiNavMeshTrivialFaceIterator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_navMesh":
            case "navMesh":
            {
                value = instance.m_navMesh;
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
            case "m_navMesh":
            case "navMesh":
            {
                if (value is null)
                {
                    instance.m_navMesh = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiNavMesh castValue)
                {
                    instance.m_navMesh = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
