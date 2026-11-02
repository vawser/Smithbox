// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class TestSimpleRecordData : HavokData<HKLib.hk2019.TestSimpleRecord> 
{
    public TestSimpleRecordData(HavokType type, HKLib.hk2019.TestSimpleRecord instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bool":
            case "bool":
            {
                if (instance.m_bool is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bool8":
            case "bool8":
            {
                if (instance.m_bool8 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uint8":
            case "uint8":
            {
                if (instance.m_uint8 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_int8":
            case "int8":
            {
                if (instance.m_int8 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uint16":
            case "uint16":
            {
                if (instance.m_uint16 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_int16":
            case "int16":
            {
                if (instance.m_int16 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uint32":
            case "uint32":
            {
                if (instance.m_uint32 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_int32":
            case "int32":
            {
                if (instance.m_int32 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uint64":
            case "uint64":
            {
                if (instance.m_uint64 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_int64":
            case "int64":
            {
                if (instance.m_int64 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_double64":
            case "double64":
            {
                if (instance.m_double64 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_float32":
            case "float32":
            {
                if (instance.m_float32 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_real":
            case "real":
            {
                if (instance.m_real is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enumClass":
            case "enumClass":
            {
                if (instance.m_enumClass is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_enumClass is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_enum":
            case "enum":
            {
                if (instance.m_enum is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_enum is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_flags is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_unsupported_ptr":
            case "unsupported_ptr":
            {
                if (instance.m_unsupported_ptr is null)
                {
                    return true;
                }
                if (instance.m_unsupported_ptr is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_unsupported_array":
            case "unsupported_array":
            {
                if (instance.m_unsupported_array is not TGet castValue) return false;
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
            case "m_bool":
            case "bool":
            {
                if (value is not bool castValue) return false;
                instance.m_bool = castValue;
                return true;
            }
            case "m_bool8":
            case "bool8":
            {
                if (value is not bool castValue) return false;
                instance.m_bool8 = castValue;
                return true;
            }
            case "m_uint8":
            case "uint8":
            {
                if (value is not byte castValue) return false;
                instance.m_uint8 = castValue;
                return true;
            }
            case "m_int8":
            case "int8":
            {
                if (value is not sbyte castValue) return false;
                instance.m_int8 = castValue;
                return true;
            }
            case "m_uint16":
            case "uint16":
            {
                if (value is not ushort castValue) return false;
                instance.m_uint16 = castValue;
                return true;
            }
            case "m_int16":
            case "int16":
            {
                if (value is not short castValue) return false;
                instance.m_int16 = castValue;
                return true;
            }
            case "m_uint32":
            case "uint32":
            {
                if (value is not uint castValue) return false;
                instance.m_uint32 = castValue;
                return true;
            }
            case "m_int32":
            case "int32":
            {
                if (value is not int castValue) return false;
                instance.m_int32 = castValue;
                return true;
            }
            case "m_uint64":
            case "uint64":
            {
                if (value is not ulong castValue) return false;
                instance.m_uint64 = castValue;
                return true;
            }
            case "m_int64":
            case "int64":
            {
                if (value is not long castValue) return false;
                instance.m_int64 = castValue;
                return true;
            }
            case "m_double64":
            case "double64":
            {
                if (value is not double castValue) return false;
                instance.m_double64 = castValue;
                return true;
            }
            case "m_float32":
            case "float32":
            {
                if (value is not float castValue) return false;
                instance.m_float32 = castValue;
                return true;
            }
            case "m_real":
            case "real":
            {
                if (value is not float castValue) return false;
                instance.m_real = castValue;
                return true;
            }
            case "m_enumClass":
            case "enumClass":
            {
                if (value is HKLib.hk2019.TestEnumClass castValue)
                {
                    instance.m_enumClass = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_enumClass = (HKLib.hk2019.TestEnumClass)intValue;
                    return true;
                }
                return false;
            }
            case "m_enum":
            case "enum":
            {
                if (value is HKLib.hk2019.TestEnum.Enum castValue)
                {
                    instance.m_enum = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_enum = (HKLib.hk2019.TestEnum.Enum)intValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (value is HKLib.hk2019.TestFlags.Bits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (HKLib.hk2019.TestFlags.Bits)byteValue;
                    return true;
                }
                return false;
            }
            case "m_unsupported_ptr":
            case "unsupported_ptr":
            {
                if (value is null)
                {
                    instance.m_unsupported_ptr = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkReferencedObject castValue)
                {
                    instance.m_unsupported_ptr = castValue;
                    return true;
                }
                return false;
            }
            case "m_unsupported_array":
            case "unsupported_array":
            {
                if (value is not List<bool> castValue) return false;
                instance.m_unsupported_array = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
