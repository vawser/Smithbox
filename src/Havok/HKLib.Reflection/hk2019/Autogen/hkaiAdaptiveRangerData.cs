// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiAdaptiveRangerData : HavokData<HKLib.hk2019.hkaiAdaptiveRanger> 
{
    public hkaiAdaptiveRangerData(HavokType type, HKLib.hk2019.hkaiAdaptiveRanger instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_curRange":
            case "curRange":
            {
                if (instance.m_curRange is not TGet castValue) return false;
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
            case "m_curRange":
            case "curRange":
            {
                if (value is not float castValue) return false;
                instance.m_curRange = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
