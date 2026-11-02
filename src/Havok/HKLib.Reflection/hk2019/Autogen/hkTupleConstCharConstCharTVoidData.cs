// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkTupleConstCharConstCharTVoidData : HavokData<HKLib.hk2019.hkTupleConstCharConstCharTVoid> 
{
    public hkTupleConstCharConstCharTVoidData(HavokType type, HKLib.hk2019.hkTupleConstCharConstCharTVoid instance) : base(type, instance) {}

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
            case "m_1":
            case "1":
            {
                if (instance.m_1 is null)
                {
                    return true;
                }
                if (instance.m_1 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_2":
            case "2":
            {
                if (instance.m_2 is null)
                {
                    return true;
                }
                if (instance.m_2 is TGet castValue)
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
            case "m_1":
            case "1":
            {
                if (value is null)
                {
                    instance.m_1 = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_1 = castValue;
                    return true;
                }
                return false;
            }
            case "m_2":
            case "2":
            {
                if (value is null)
                {
                    instance.m_2 = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_2 = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
