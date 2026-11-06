// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class TestComplexContainerRecordData : Havok.Formats.HavokFieldAccess<Havok.hk2019.TestComplexContainerRecord>
{
    private static readonly System.Reflection.FieldInfo _one_cstringInfo = typeof(Havok.hk2019.TestComplexContainerRecord).GetField("m_one_cstring")!;
    private static readonly System.Reflection.FieldInfo _one_stringInfo = typeof(Havok.hk2019.TestComplexContainerRecord).GetField("m_one_string")!;
    private static readonly System.Reflection.FieldInfo _one_unsupported_ptrInfo = typeof(Havok.hk2019.TestComplexContainerRecord).GetField("m_one_unsupported_ptr")!;
    private static readonly System.Reflection.FieldInfo _one_unsupported_arrayInfo = typeof(Havok.hk2019.TestComplexContainerRecord).GetField("m_one_unsupported_array")!;
    private static readonly System.Reflection.FieldInfo _one_recordInfo = typeof(Havok.hk2019.TestComplexContainerRecord).GetField("m_one_record")!;
    private static readonly System.Reflection.FieldInfo _many_cstringsInfo = typeof(Havok.hk2019.TestComplexContainerRecord).GetField("m_many_cstrings")!;
    private static readonly System.Reflection.FieldInfo _many_stringsInfo = typeof(Havok.hk2019.TestComplexContainerRecord).GetField("m_many_strings")!;
    private static readonly System.Reflection.FieldInfo _many_unsupported_ptrsInfo = typeof(Havok.hk2019.TestComplexContainerRecord).GetField("m_many_unsupported_ptrs")!;
    private static readonly System.Reflection.FieldInfo _many_unsupported_arraysInfo = typeof(Havok.hk2019.TestComplexContainerRecord).GetField("m_many_unsupported_arrays")!;
    private static readonly System.Reflection.FieldInfo _many_recordsInfo = typeof(Havok.hk2019.TestComplexContainerRecord).GetField("m_many_records")!;
    public TestComplexContainerRecordData(Havok.hk2019.TestComplexContainerRecord instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_one_cstring":
            case "one_cstring":
            {
                value = instance.m_one_cstring;
                return true;
            }
            case "m_one_string":
            case "one_string":
            {
                value = instance.m_one_string;
                return true;
            }
            case "m_one_unsupported_ptr":
            case "one_unsupported_ptr":
            {
                value = instance.m_one_unsupported_ptr;
                return true;
            }
            case "m_one_unsupported_array":
            case "one_unsupported_array":
            {
                value = instance.m_one_unsupported_array;
                return true;
            }
            case "m_one_record":
            case "one_record":
            {
                value = instance.m_one_record;
                return true;
            }
            case "m_many_cstrings":
            case "many_cstrings":
            {
                value = instance.m_many_cstrings;
                return true;
            }
            case "m_many_strings":
            case "many_strings":
            {
                value = instance.m_many_strings;
                return true;
            }
            case "m_many_unsupported_ptrs":
            case "many_unsupported_ptrs":
            {
                value = instance.m_many_unsupported_ptrs;
                return true;
            }
            case "m_many_unsupported_arrays":
            case "many_unsupported_arrays":
            {
                value = instance.m_many_unsupported_arrays;
                return true;
            }
            case "m_many_records":
            case "many_records":
            {
                value = instance.m_many_records;
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
            case "m_one_cstring":
            case "one_cstring":
            {
                if (value is not string?[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_cstringInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_one_string":
            case "one_string":
            {
                if (value is not string?[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_stringInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_one_unsupported_ptr":
            case "one_unsupported_ptr":
            {
                if (value is not Havok.hk2019.hkReferencedObject?[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_unsupported_ptrInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_one_unsupported_array":
            case "one_unsupported_array":
            {
                if (value is not List<bool>[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_unsupported_arrayInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_one_record":
            case "one_record":
            {
                if (value is not Havok.hk2019.TestComplexRecord[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_recordInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_many_cstrings":
            case "many_cstrings":
            {
                if (value is not string?[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_cstringsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_many_strings":
            case "many_strings":
            {
                if (value is not string?[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_stringsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_many_unsupported_ptrs":
            case "many_unsupported_ptrs":
            {
                if (value is not Havok.hk2019.hkReferencedObject?[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_unsupported_ptrsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_many_unsupported_arrays":
            case "many_unsupported_arrays":
            {
                if (value is not List<bool>[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_unsupported_arraysInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_many_records":
            case "many_records":
            {
                if (value is not Havok.hk2019.TestComplexRecord[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_recordsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
