// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpEntitySmallArraySerializeOverrideTypeData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpEntitySmallArraySerializeOverrideType>
{
    public hkpEntitySmallArraySerializeOverrideTypeData(Havok.hk55.hkpEntitySmallArraySerializeOverrideType instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            case "m_size":
            case "size":
            {
                value = instance.m_size;
                return true;
            }
            case "m_capacityAndFlags":
            case "capacityAndFlags":
            {
                value = instance.m_capacityAndFlags;
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
            case "m_data":
            case "data":
            {
                if (value is null)
                {
                    instance.m_data = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_data = castValue;
                    return true;
                }
                return false;
            }
            case "m_size":
            case "size":
            {
                if (value is not ushort castValue) return false;
                instance.m_size = castValue;
                return true;
            }
            case "m_capacityAndFlags":
            case "capacityAndFlags":
            {
                if (value is not ushort castValue) return false;
                instance.m_capacityAndFlags = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
