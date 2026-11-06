// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkDeleteTypeInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hk.DeleteTypeInfo>
{
    public hkDeleteTypeInfoData(Havok.hk2019.hk.DeleteTypeInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_func":
            case "func":
            {
                value = instance.m_func;
                return true;
            }
            case "m_allocator":
            case "allocator":
            {
                value = instance.m_allocator;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
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
