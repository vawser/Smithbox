// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiDynamicNavMeshQueryMediatorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiDynamicNavMeshQueryMediator>
{
    public hkaiDynamicNavMeshQueryMediatorData(Havok.hk2018.hkaiDynamicNavMeshQueryMediator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_collection":
            case "collection":
            {
                value = instance.m_collection;
                return true;
            }
            case "m_cutAabbTolerance":
            case "cutAabbTolerance":
            {
                value = instance.m_cutAabbTolerance;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
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
                if (value is Havok.hk2018.hkaiStreamingCollection castValue)
                {
                    instance.m_collection = castValue;
                    return true;
                }
                return false;
            }
            case "m_cutAabbTolerance":
            case "cutAabbTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_cutAabbTolerance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
