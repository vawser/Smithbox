// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiDynamicNavMeshQueryMediatorData : HavokData<hkaiDynamicNavMeshQueryMediator> 
{
    public hkaiDynamicNavMeshQueryMediatorData(HavokType type, hkaiDynamicNavMeshQueryMediator instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collection":
            case "collection":
            {
                if (instance.m_collection is null)
                {
                    return true;
                }
                if (instance.m_collection is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_cutAabbTolerance":
            case "cutAabbTolerance":
            {
                if (instance.m_cutAabbTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
                if (value is hkaiStreamingCollection castValue)
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
