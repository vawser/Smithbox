// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavVolumeInstanceQueryMediatorData : HavokData<HKLib.hk2019.hkaiNavVolumeInstanceQueryMediator> 
{
    public hkaiNavVolumeInstanceQueryMediatorData(HavokType type, HKLib.hk2019.hkaiNavVolumeInstanceQueryMediator instance) : base(type, instance) {}

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
            case "m_navVolumeInstance":
            case "navVolumeInstance":
            {
                if (instance.m_navVolumeInstance is null)
                {
                    return true;
                }
                if (instance.m_navVolumeInstance is TGet castValue)
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
            case "m_navVolumeInstance":
            case "navVolumeInstance":
            {
                if (value is null)
                {
                    instance.m_navVolumeInstance = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiNavVolumeInstance castValue)
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
