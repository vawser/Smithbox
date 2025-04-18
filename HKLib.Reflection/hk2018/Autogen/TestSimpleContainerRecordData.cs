// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class TestSimpleContainerRecordData : HavokData<TestSimpleContainerRecord> 
{
    private static readonly System.Reflection.FieldInfo _one_boolInfo = typeof(TestSimpleContainerRecord).GetField("m_one_bool")!;
    private static readonly System.Reflection.FieldInfo _one_bool8Info = typeof(TestSimpleContainerRecord).GetField("m_one_bool8")!;
    private static readonly System.Reflection.FieldInfo _one_uint8Info = typeof(TestSimpleContainerRecord).GetField("m_one_uint8")!;
    private static readonly System.Reflection.FieldInfo _one_int8Info = typeof(TestSimpleContainerRecord).GetField("m_one_int8")!;
    private static readonly System.Reflection.FieldInfo _one_uint16Info = typeof(TestSimpleContainerRecord).GetField("m_one_uint16")!;
    private static readonly System.Reflection.FieldInfo _one_int16Info = typeof(TestSimpleContainerRecord).GetField("m_one_int16")!;
    private static readonly System.Reflection.FieldInfo _one_uint32Info = typeof(TestSimpleContainerRecord).GetField("m_one_uint32")!;
    private static readonly System.Reflection.FieldInfo _one_int32Info = typeof(TestSimpleContainerRecord).GetField("m_one_int32")!;
    private static readonly System.Reflection.FieldInfo _one_uint64Info = typeof(TestSimpleContainerRecord).GetField("m_one_uint64")!;
    private static readonly System.Reflection.FieldInfo _one_int64Info = typeof(TestSimpleContainerRecord).GetField("m_one_int64")!;
    private static readonly System.Reflection.FieldInfo _one_double64Info = typeof(TestSimpleContainerRecord).GetField("m_one_double64")!;
    private static readonly System.Reflection.FieldInfo _one_float32Info = typeof(TestSimpleContainerRecord).GetField("m_one_float32")!;
    private static readonly System.Reflection.FieldInfo _one_realInfo = typeof(TestSimpleContainerRecord).GetField("m_one_real")!;
    private static readonly System.Reflection.FieldInfo _one_unsupported_ptrInfo = typeof(TestSimpleContainerRecord).GetField("m_one_unsupported_ptr")!;
    private static readonly System.Reflection.FieldInfo _one_unsupported_arrayInfo = typeof(TestSimpleContainerRecord).GetField("m_one_unsupported_array")!;
    private static readonly System.Reflection.FieldInfo _many_boolsInfo = typeof(TestSimpleContainerRecord).GetField("m_many_bools")!;
    private static readonly System.Reflection.FieldInfo _many_bool8sInfo = typeof(TestSimpleContainerRecord).GetField("m_many_bool8s")!;
    private static readonly System.Reflection.FieldInfo _many_uint8sInfo = typeof(TestSimpleContainerRecord).GetField("m_many_uint8s")!;
    private static readonly System.Reflection.FieldInfo _many_int8sInfo = typeof(TestSimpleContainerRecord).GetField("m_many_int8s")!;
    private static readonly System.Reflection.FieldInfo _many_uint16sInfo = typeof(TestSimpleContainerRecord).GetField("m_many_uint16s")!;
    private static readonly System.Reflection.FieldInfo _many_int16sInfo = typeof(TestSimpleContainerRecord).GetField("m_many_int16s")!;
    private static readonly System.Reflection.FieldInfo _many_uint32sInfo = typeof(TestSimpleContainerRecord).GetField("m_many_uint32s")!;
    private static readonly System.Reflection.FieldInfo _many_int32sInfo = typeof(TestSimpleContainerRecord).GetField("m_many_int32s")!;
    private static readonly System.Reflection.FieldInfo _many_uint64sInfo = typeof(TestSimpleContainerRecord).GetField("m_many_uint64s")!;
    private static readonly System.Reflection.FieldInfo _many_int64sInfo = typeof(TestSimpleContainerRecord).GetField("m_many_int64s")!;
    private static readonly System.Reflection.FieldInfo _many_double64sInfo = typeof(TestSimpleContainerRecord).GetField("m_many_double64s")!;
    private static readonly System.Reflection.FieldInfo _many_float32sInfo = typeof(TestSimpleContainerRecord).GetField("m_many_float32s")!;
    private static readonly System.Reflection.FieldInfo _many_realsInfo = typeof(TestSimpleContainerRecord).GetField("m_many_reals")!;
    private static readonly System.Reflection.FieldInfo _many_unsupported_ptrsInfo = typeof(TestSimpleContainerRecord).GetField("m_many_unsupported_ptrs")!;
    private static readonly System.Reflection.FieldInfo _many_unsupported_arraysInfo = typeof(TestSimpleContainerRecord).GetField("m_many_unsupported_arrays")!;
    public TestSimpleContainerRecordData(HavokType type, TestSimpleContainerRecord instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_one_bool":
            case "one_bool":
            {
                if (instance.m_one_bool is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_one_bool8":
            case "one_bool8":
            {
                if (instance.m_one_bool8 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_one_uint8":
            case "one_uint8":
            {
                if (instance.m_one_uint8 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_one_int8":
            case "one_int8":
            {
                if (instance.m_one_int8 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_one_uint16":
            case "one_uint16":
            {
                if (instance.m_one_uint16 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_one_int16":
            case "one_int16":
            {
                if (instance.m_one_int16 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_one_uint32":
            case "one_uint32":
            {
                if (instance.m_one_uint32 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_one_int32":
            case "one_int32":
            {
                if (instance.m_one_int32 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_one_uint64":
            case "one_uint64":
            {
                if (instance.m_one_uint64 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_one_int64":
            case "one_int64":
            {
                if (instance.m_one_int64 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_one_double64":
            case "one_double64":
            {
                if (instance.m_one_double64 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_one_float32":
            case "one_float32":
            {
                if (instance.m_one_float32 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_one_real":
            case "one_real":
            {
                if (instance.m_one_real is not TGet castValue) return false;
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
            case "m_many_bools":
            case "many_bools":
            {
                if (instance.m_many_bools is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_many_bool8s":
            case "many_bool8s":
            {
                if (instance.m_many_bool8s is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_many_uint8s":
            case "many_uint8s":
            {
                if (instance.m_many_uint8s is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_many_int8s":
            case "many_int8s":
            {
                if (instance.m_many_int8s is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_many_uint16s":
            case "many_uint16s":
            {
                if (instance.m_many_uint16s is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_many_int16s":
            case "many_int16s":
            {
                if (instance.m_many_int16s is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_many_uint32s":
            case "many_uint32s":
            {
                if (instance.m_many_uint32s is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_many_int32s":
            case "many_int32s":
            {
                if (instance.m_many_int32s is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_many_uint64s":
            case "many_uint64s":
            {
                if (instance.m_many_uint64s is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_many_int64s":
            case "many_int64s":
            {
                if (instance.m_many_int64s is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_many_double64s":
            case "many_double64s":
            {
                if (instance.m_many_double64s is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_many_float32s":
            case "many_float32s":
            {
                if (instance.m_many_float32s is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_many_reals":
            case "many_reals":
            {
                if (instance.m_many_reals is not TGet castValue) return false;
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
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_one_bool":
            case "one_bool":
            {
                if (value is not bool[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_boolInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_one_bool8":
            case "one_bool8":
            {
                if (value is not bool[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_bool8Info.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_one_uint8":
            case "one_uint8":
            {
                if (value is not byte[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_uint8Info.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_one_int8":
            case "one_int8":
            {
                if (value is not sbyte[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_int8Info.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_one_uint16":
            case "one_uint16":
            {
                if (value is not ushort[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_uint16Info.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_one_int16":
            case "one_int16":
            {
                if (value is not short[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_int16Info.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_one_uint32":
            case "one_uint32":
            {
                if (value is not uint[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_uint32Info.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_one_int32":
            case "one_int32":
            {
                if (value is not int[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_int32Info.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_one_uint64":
            case "one_uint64":
            {
                if (value is not ulong[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_uint64Info.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_one_int64":
            case "one_int64":
            {
                if (value is not long[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_int64Info.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_one_double64":
            case "one_double64":
            {
                if (value is not double[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_double64Info.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_one_float32":
            case "one_float32":
            {
                if (value is not float[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_float32Info.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_one_real":
            case "one_real":
            {
                if (value is not float[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_realInfo.SetValue(instance, value);
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
                if (value is not hkReferencedObject?[] castValue || castValue.Length != 1) return false;
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
            case "m_many_bools":
            case "many_bools":
            {
                if (value is not bool[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_boolsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_many_bool8s":
            case "many_bool8s":
            {
                if (value is not bool[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_bool8sInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_many_uint8s":
            case "many_uint8s":
            {
                if (value is not byte[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_uint8sInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_many_int8s":
            case "many_int8s":
            {
                if (value is not sbyte[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_int8sInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_many_uint16s":
            case "many_uint16s":
            {
                if (value is not ushort[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_uint16sInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_many_int16s":
            case "many_int16s":
            {
                if (value is not short[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_int16sInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_many_uint32s":
            case "many_uint32s":
            {
                if (value is not uint[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_uint32sInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_many_int32s":
            case "many_int32s":
            {
                if (value is not int[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_int32sInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_many_uint64s":
            case "many_uint64s":
            {
                if (value is not ulong[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_uint64sInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_many_int64s":
            case "many_int64s":
            {
                if (value is not long[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_int64sInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_many_double64s":
            case "many_double64s":
            {
                if (value is not double[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_double64sInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_many_float32s":
            case "many_float32s":
            {
                if (value is not float[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_float32sInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_many_reals":
            case "many_reals":
            {
                if (value is not float[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_realsInfo.SetValue(instance, value);
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
                if (value is not hkReferencedObject?[] castValue || castValue.Length != 10) return false;
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
            default:
            return false;
        }
    }

}
