// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshCollectionQueryMediatorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshCollectionQueryMediator>
{
    public hkaiNavMeshCollectionQueryMediatorData(Havok.hk2019.hkaiNavMeshCollectionQueryMediator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_cutAabbTolerance":
            case "cutAabbTolerance":
            {
                value = instance.m_cutAabbTolerance;
                return true;
            }
            case "m_collection":
            case "collection":
            {
                value = instance.m_collection;
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
            case "m_cutAabbTolerance":
            case "cutAabbTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_cutAabbTolerance = castValue;
                return true;
            }
            case "m_collection":
            case "collection":
            {
                if (value is null)
                {
                    instance.m_collection = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiStreamingCollection castValue)
                {
                    instance.m_collection = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
