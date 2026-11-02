// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpRefWorldCinfoData : HavokData<HKLib.hk2019.hknpRefWorldCinfo> 
{
    public hknpRefWorldCinfoData(HavokType type, HKLib.hk2019.hknpRefWorldCinfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_info":
            case "info":
            {
                if (instance.m_info is not TGet castValue) return false;
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
            case "m_info":
            case "info":
            {
                if (value is not HKLib.hk2019.hknpWorldCinfo castValue) return false;
                instance.m_info = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
