// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkWeakPtrTest;

namespace HKLib.Reflection.hk2018;

internal class hkWeakPtrTestBarData : HavokData<Bar> 
{
    public hkWeakPtrTestBarData(HavokType type, Bar instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_strongPtr":
            case "strongPtr":
            {
                if (instance.m_strongPtr is null)
                {
                    return true;
                }
                if (instance.m_strongPtr is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_weakPtr":
            case "weakPtr":
            {
                if (instance.m_weakPtr is null)
                {
                    return true;
                }
                if (instance.m_weakPtr is TGet castValue)
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
            case "m_strongPtr":
            case "strongPtr":
            {
                if (value is null)
                {
                    instance.m_strongPtr = default;
                    return true;
                }
                if (value is Foo castValue)
                {
                    instance.m_strongPtr = castValue;
                    return true;
                }
                return false;
            }
            case "m_weakPtr":
            case "weakPtr":
            {
                if (value is null)
                {
                    instance.m_weakPtr = default;
                    return true;
                }
                if (value is Foo castValue)
                {
                    instance.m_weakPtr = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
