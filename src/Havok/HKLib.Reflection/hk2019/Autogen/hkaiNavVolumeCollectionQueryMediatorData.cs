// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavVolumeCollectionQueryMediatorData : HavokData<HKLib.hk2019.hkaiNavVolumeCollectionQueryMediator> 
{
    public hkaiNavVolumeCollectionQueryMediatorData(HavokType type, HKLib.hk2019.hkaiNavVolumeCollectionQueryMediator instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_collection":
            case "collection":
            {
                if (value is null)
                {
                    instance.m_collection = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiStreamingCollection castValue)
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
