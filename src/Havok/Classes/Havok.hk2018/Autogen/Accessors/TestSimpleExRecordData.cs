// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class TestSimpleExRecordData : Havok.Formats.HavokFieldAccess<Havok.hk2018.TestSimpleExRecord>
{
    public TestSimpleExRecordData(Havok.hk2018.TestSimpleExRecord instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bool":
            case "bool":
            {
                value = instance.m_bool;
                return true;
            }
            case "m_bool8":
            case "bool8":
            {
                value = instance.m_bool8;
                return true;
            }
            case "m_uint8":
            case "uint8":
            {
                value = instance.m_uint8;
                return true;
            }
            case "m_int8":
            case "int8":
            {
                value = instance.m_int8;
                return true;
            }
            case "m_uint16":
            case "uint16":
            {
                value = instance.m_uint16;
                return true;
            }
            case "m_int16":
            case "int16":
            {
                value = instance.m_int16;
                return true;
            }
            case "m_uint32":
            case "uint32":
            {
                value = instance.m_uint32;
                return true;
            }
            case "m_int32":
            case "int32":
            {
                value = instance.m_int32;
                return true;
            }
            case "m_uint64":
            case "uint64":
            {
                value = instance.m_uint64;
                return true;
            }
            case "m_int64":
            case "int64":
            {
                value = instance.m_int64;
                return true;
            }
            case "m_double64":
            case "double64":
            {
                value = instance.m_double64;
                return true;
            }
            case "m_float32":
            case "float32":
            {
                value = instance.m_float32;
                return true;
            }
            case "m_real":
            case "real":
            {
                value = instance.m_real;
                return true;
            }
            case "m_unsupported_ptr":
            case "unsupported_ptr":
            {
                value = instance.m_unsupported_ptr;
                return true;
            }
            case "m_unsupported_array":
            case "unsupported_array":
            {
                value = instance.m_unsupported_array;
                return true;
            }
            case "m_nested":
            case "nested":
            {
                value = instance.m_nested;
                return true;
            }
            case "m_record":
            case "record":
            {
                value = instance.m_record;
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
                if (value is Havok.hk2018.hkReferencedObject castValue)
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
            case "m_nested":
            case "nested":
            {
                if (value is not Havok.hk2018.TestSimpleExRecord.Nested castValue) return false;
                instance.m_nested = castValue;
                return true;
            }
            case "m_record":
            case "record":
            {
                if (value is not Havok.hk2018.TestSimpleRecord castValue) return false;
                instance.m_record = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
