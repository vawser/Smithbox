// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkAtomic;

namespace HKLib.Reflection.hk2018;

internal class hkaiCopyOnWritePtrData<T> : HavokData<hkaiCopyOnWritePtr<T>> 
{
    public hkaiCopyOnWritePtrData(HavokType type, hkaiCopyOnWritePtr<T> instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_ptr":
            case "ptr":
            {
                if (instance.m_ptr is null)
                {
                    return true;
                }
                if (instance.m_ptr is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_ptrState":
            case "ptrState":
            {
                if (instance.m_ptrState is not TGet castValue) return false;
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
            case "m_ptr":
            case "ptr":
            {
                if (value is null)
                {
                    instance.m_ptr = default;
                    return true;
                }
                if (value is T castValue)
                {
                    instance.m_ptr = castValue;
                    return true;
                }
                return false;
            }
            case "m_ptrState":
            case "ptrState":
            {
                if (value is not Variable<byte> castValue) return false;
                instance.m_ptrState = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
