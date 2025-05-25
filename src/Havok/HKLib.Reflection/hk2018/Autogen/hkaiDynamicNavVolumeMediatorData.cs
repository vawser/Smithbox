// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiDynamicNavVolumeMediatorData : HavokData<hkaiDynamicNavVolumeMediator> 
{
    public hkaiDynamicNavVolumeMediatorData(HavokType type, hkaiDynamicNavVolumeMediator instance) : base(type, instance) {}

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
            default:
            return false;
        }
    }

}
