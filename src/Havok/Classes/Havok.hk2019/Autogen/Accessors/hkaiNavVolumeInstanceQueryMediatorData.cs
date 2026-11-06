// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavVolumeInstanceQueryMediatorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavVolumeInstanceQueryMediator>
{
    public hkaiNavVolumeInstanceQueryMediatorData(Havok.hk2019.hkaiNavVolumeInstanceQueryMediator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_cellIterator":
            case "cellIterator":
            {
                value = instance.m_cellIterator;
                return true;
            }
            case "m_navVolumeInstance":
            case "navVolumeInstance":
            {
                value = instance.m_navVolumeInstance;
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
            case "m_cellIterator":
            case "cellIterator":
            {
                if (value is null)
                {
                    instance.m_cellIterator = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiNavVolumeCellIterator castValue)
                {
                    instance.m_cellIterator = castValue;
                    return true;
                }
                return false;
            }
            case "m_navVolumeInstance":
            case "navVolumeInstance":
            {
                if (value is null)
                {
                    instance.m_navVolumeInstance = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiNavVolumeInstance castValue)
                {
                    instance.m_navVolumeInstance = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
