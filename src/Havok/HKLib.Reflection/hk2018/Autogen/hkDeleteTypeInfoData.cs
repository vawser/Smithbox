// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hk;

namespace HKLib.Reflection.hk2018;

internal class hkDeleteTypeInfoData : HavokData<DeleteTypeInfo> 
{
    public hkDeleteTypeInfoData(HavokType type, DeleteTypeInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_func":
            case "func":
            {
                if (instance.m_func is null)
                {
                    return true;
                }
                if (instance.m_func is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_allocator":
            case "allocator":
            {
                if (instance.m_allocator is null)
                {
                    return true;
                }
                if (instance.m_allocator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_data":
            case "data":
            {
                if (instance.m_data is not TGet castValue) return false;
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
            case "m_func":
            case "func":
            {
                if (value is null)
                {
                    instance.m_func = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_func = castValue;
                    return true;
                }
                return false;
            }
            case "m_allocator":
            case "allocator":
            {
                if (value is null)
                {
                    instance.m_allocator = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_allocator = castValue;
                    return true;
                }
                return false;
            }
            case "m_data":
            case "data":
            {
                if (value is not ulong castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
