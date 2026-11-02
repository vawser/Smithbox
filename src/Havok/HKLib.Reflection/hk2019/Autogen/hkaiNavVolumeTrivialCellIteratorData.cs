// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavVolumeTrivialCellIteratorData : HavokData<HKLib.hk2019.hkaiNavVolumeTrivialCellIterator> 
{
    public hkaiNavVolumeTrivialCellIteratorData(HavokType type, HKLib.hk2019.hkaiNavVolumeTrivialCellIterator instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
