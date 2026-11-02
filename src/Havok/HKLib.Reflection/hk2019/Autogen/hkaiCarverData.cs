// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiCarverData : HavokData<HKLib.hk2019.hkaiCarver> 
{
    public hkaiCarverData(HavokType type, HKLib.hk2019.hkaiCarver instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_volume":
            case "volume":
            {
                if (instance.m_volume is null)
                {
                    return true;
                }
                if (instance.m_volume is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_flags is TGet uintValue)
                {
                    value = uintValue;
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
            case "m_volume":
            case "volume":
            {
                if (value is null)
                {
                    instance.m_volume = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiVolume castValue)
                {
                    instance.m_volume = castValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (value is HKLib.hk2019.hkaiCarver.FlagBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_flags = (HKLib.hk2019.hkaiCarver.FlagBits)uintValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
