// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavVolumeQueryMediatorData : HavokData<HKLib.hk2019.hkaiNavVolumeQueryMediator> 
{
    public hkaiNavVolumeQueryMediatorData(HavokType type, HKLib.hk2019.hkaiNavVolumeQueryMediator instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_cellIterator":
            case "cellIterator":
            {
                if (instance.m_cellIterator is null)
                {
                    return true;
                }
                if (instance.m_cellIterator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_navVolume":
            case "navVolume":
            {
                if (instance.m_navVolume is null)
                {
                    return true;
                }
                if (instance.m_navVolume is TGet castValue)
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
            case "m_cellIterator":
            case "cellIterator":
            {
                if (value is null)
                {
                    instance.m_cellIterator = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiNavVolumeCellIterator castValue)
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
                if (value is HKLib.hk2019.hkaiNavVolume castValue)
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
