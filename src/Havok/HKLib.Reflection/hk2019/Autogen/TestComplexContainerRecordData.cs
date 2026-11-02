// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class TestComplexContainerRecordData : HavokData<HKLib.hk2019.TestComplexContainerRecord> 
{
    private static readonly System.Reflection.FieldInfo _one_cstringInfo = typeof(HKLib.hk2019.TestComplexContainerRecord).GetField("m_one_cstring")!;
    private static readonly System.Reflection.FieldInfo _one_stringInfo = typeof(HKLib.hk2019.TestComplexContainerRecord).GetField("m_one_string")!;
    private static readonly System.Reflection.FieldInfo _one_unsupported_ptrInfo = typeof(HKLib.hk2019.TestComplexContainerRecord).GetField("m_one_unsupported_ptr")!;
    private static readonly System.Reflection.FieldInfo _one_unsupported_arrayInfo = typeof(HKLib.hk2019.TestComplexContainerRecord).GetField("m_one_unsupported_array")!;
    private static readonly System.Reflection.FieldInfo _one_recordInfo = typeof(HKLib.hk2019.TestComplexContainerRecord).GetField("m_one_record")!;
    private static readonly System.Reflection.FieldInfo _many_cstringsInfo = typeof(HKLib.hk2019.TestComplexContainerRecord).GetField("m_many_cstrings")!;
    private static readonly System.Reflection.FieldInfo _many_stringsInfo = typeof(HKLib.hk2019.TestComplexContainerRecord).GetField("m_many_strings")!;
    private static readonly System.Reflection.FieldInfo _many_unsupported_ptrsInfo = typeof(HKLib.hk2019.TestComplexContainerRecord).GetField("m_many_unsupported_ptrs")!;
    private static readonly System.Reflection.FieldInfo _many_unsupported_arraysInfo = typeof(HKLib.hk2019.TestComplexContainerRecord).GetField("m_many_unsupported_arrays")!;
    private static readonly System.Reflection.FieldInfo _many_recordsInfo = typeof(HKLib.hk2019.TestComplexContainerRecord).GetField("m_many_records")!;
    public TestComplexContainerRecordData(HavokType type, HKLib.hk2019.TestComplexContainerRecord instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_one_cstring":
            case "one_cstring":
            {
                if (instance.m_one_cstring is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_one_string":
            case "one_string":
            {
                if (instance.m_one_string is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_one_unsupported_ptr":
            case "one_unsupported_ptr":
            {
                if (instance.m_one_unsupported_ptr is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_one_unsupported_array":
            case "one_unsupported_array":
            {
                if (instance.m_one_unsupported_array is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_one_record":
            case "one_record":
            {
                if (instance.m_one_record is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_many_cstrings":
            case "many_cstrings":
            {
                if (instance.m_many_cstrings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_many_strings":
            case "many_strings":
            {
                if (instance.m_many_strings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_many_unsupported_ptrs":
            case "many_unsupported_ptrs":
            {
                if (instance.m_many_unsupported_ptrs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_many_unsupported_arrays":
            case "many_unsupported_arrays":
            {
                if (instance.m_many_unsupported_arrays is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_many_records":
            case "many_records":
            {
                if (instance.m_many_records is not TGet castValue) return false;
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
                if (value is not HKLib.hk2019.hkReferencedObject?[] castValue || castValue.Length != 1) return false;
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
                if (value is not HKLib.hk2019.TestComplexRecord[] castValue || castValue.Length != 1) return false;
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
                if (value is not HKLib.hk2019.hkReferencedObject?[] castValue || castValue.Length != 10) return false;
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
                if (value is not HKLib.hk2019.TestComplexRecord[] castValue || castValue.Length != 10) return false;
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
