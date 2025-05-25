// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkPtrAndIntData<PTYPE> : HavokData<hkPtrAndInt<PTYPE>> 
{
    public hkPtrAndIntData(HavokType type, hkPtrAndInt<PTYPE> instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_ptrAndInt":
            case "ptrAndInt":
            {
                if (instance.m_ptrAndInt is null)
                {
                    return true;
                }
                if (instance.m_ptrAndInt is TGet castValue)
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
            case "m_ptrAndInt":
            case "ptrAndInt":
            {
                if (value is null)
                {
                    instance.m_ptrAndInt = default;
                    return true;
                }
                if (value is PTYPE castValue)
                {
                    instance.m_ptrAndInt = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
