// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkTimerVolumeUtilsLineData : HavokData<HKLib.hk2019.hkTimerVolumeUtils.Line> 
{
    public hkTimerVolumeUtilsLineData(HavokType type, HKLib.hk2019.hkTimerVolumeUtils.Line instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_start":
            case "start":
            {
                if (instance.m_start is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_end":
            case "end":
            {
                if (instance.m_end is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_start":
            case "start":
            {
                if (value is not HKLib.hk2019.hkFloat3 castValue) return false;
                instance.m_start = castValue;
                return true;
            }
            case "m_end":
            case "end":
            {
                if (value is not HKLib.hk2019.hkFloat3 castValue) return false;
                instance.m_end = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
