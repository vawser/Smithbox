// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class TestSimpleRecordData : HavokData<TestSimpleRecord> 
{
    public TestSimpleRecordData(HavokType type, TestSimpleRecord instance) : base(type, instance) {}

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
            case "m_unsupported_ptr":
            case "unsupported_ptr":
            {
                if (value is null)
                {
                    instance.m_unsupported_ptr = default;
                    return true;
                }
                if (value is hkReferencedObject castValue)
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
