// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkTuple3Data<T0, T1, T2> : HavokData<hkTuple3<T0, T1, T2>> 
{
    public hkTuple3Data(HavokType type, hkTuple3<T0, T1, T2> instance) : base(type, instance) {}

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
                if (value is T1 castValue)
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
                if (value is T2 castValue)
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
