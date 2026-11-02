// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkTupleConstCharData : HavokData<HKLib.hk2019.hkTupleConstChar> 
{
    public hkTupleConstCharData(HavokType type, HKLib.hk2019.hkTupleConstChar instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_0":
            case "0":
            {
                if (instance.m_0 is null)
                {
                    return true;
                }
                if (instance.m_0 is TGet castValue)
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
            case "m_0":
            case "0":
            {
                if (value is null)
                {
                    instance.m_0 = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_0 = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
