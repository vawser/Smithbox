// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiDynamicNavVolumeMediatorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiDynamicNavVolumeMediator>
{
    public hkaiDynamicNavVolumeMediatorData(Havok.hk2014.hkaiDynamicNavVolumeMediator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_collection":
            case "collection":
            {
                value = instance.m_collection;
                return true;
            }
            case "m_aabbTree":
            case "aabbTree":
            {
                value = instance.m_aabbTree;
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
            case "m_collection":
            case "collection":
            {
                if (value is null)
                {
                    instance.m_collection = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiStreamingCollection castValue)
                {
                    instance.m_collection = castValue;
                    return true;
                }
                return false;
            }
            case "m_aabbTree":
            case "aabbTree":
            {
                if (value is null)
                {
                    instance.m_aabbTree = default;
                    return true;
                }
                if (value is Havok.hk2014.hkcdDynamicAabbTree castValue)
                {
                    instance.m_aabbTree = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
