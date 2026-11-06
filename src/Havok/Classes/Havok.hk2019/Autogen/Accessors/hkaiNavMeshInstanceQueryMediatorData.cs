// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshInstanceQueryMediatorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshInstanceQueryMediator>
{
    public hkaiNavMeshInstanceQueryMediatorData(Havok.hk2019.hkaiNavMeshInstanceQueryMediator instance) : base(instance) { }

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
            case "m_meshInstance":
            case "meshInstance":
            {
                value = instance.m_meshInstance;
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
            case "m_meshInstance":
            case "meshInstance":
            {
                if (value is null)
                {
                    instance.m_meshInstance = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiNavMeshInstance castValue)
                {
                    instance.m_meshInstance = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
