// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkTuple2Data<T0, T1> : HavokData<HKLib.hk2019.hkTuple2<T0, T1>> 
{
    public hkTuple2Data(HavokType type, HKLib.hk2019.hkTuple2<T0, T1> instance) : base(type, instance) {}

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
                if (value is T0 castValue)
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
                if (value is T0 castValue)
                {
                    instance.m_1 = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
