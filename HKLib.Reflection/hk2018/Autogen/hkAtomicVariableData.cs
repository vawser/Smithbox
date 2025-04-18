// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkAtomic;

namespace HKLib.Reflection.hk2018;

internal class hkAtomicVariableData<T> : HavokData<Variable<T>> 
{
    public hkAtomicVariableData(HavokType type, Variable<T> instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_val":
            case "val":
            {
                if (instance.m_val is null)
                {
                    return true;
                }
                if (instance.m_val is TGet castValue)
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
            case "m_val":
            case "val":
            {
                if (value is null)
                {
                    instance.m_val = default!;
                    return true;
                }
                if (value is T castValue)
                {
                    instance.m_val = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
