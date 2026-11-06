// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavVolumeQueryMediatorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavVolumeQueryMediator>
{
    public hkaiNavVolumeQueryMediatorData(Havok.hk2019.hkaiNavVolumeQueryMediator instance) : base(instance) { }

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
            case "m_navVolume":
            case "navVolume":
            {
                value = instance.m_navVolume;
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
            case "m_navVolume":
            case "navVolume":
            {
                if (value is null)
                {
                    instance.m_navVolume = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiNavVolume castValue)
                {
                    instance.m_navVolume = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
