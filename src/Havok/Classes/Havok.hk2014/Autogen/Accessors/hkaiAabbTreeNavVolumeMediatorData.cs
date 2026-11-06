// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiAabbTreeNavVolumeMediatorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiAabbTreeNavVolumeMediator>
{
    public hkaiAabbTreeNavVolumeMediatorData(Havok.hk2014.hkaiAabbTreeNavVolumeMediator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_navVolume":
            case "navVolume":
            {
                value = instance.m_navVolume;
                return true;
            }
            case "m_tree":
            case "tree":
            {
                value = instance.m_tree;
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
            case "m_navVolume":
            case "navVolume":
            {
                if (value is null)
                {
                    instance.m_navVolume = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiNavVolume castValue)
                {
                    instance.m_navVolume = castValue;
                    return true;
                }
                return false;
            }
            case "m_tree":
            case "tree":
            {
                if (value is null)
                {
                    instance.m_tree = default;
                    return true;
                }
                if (value is Havok.hk2014.hkcdStaticAabbTree castValue)
                {
                    instance.m_tree = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
