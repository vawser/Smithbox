// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestTypeVmRecordData : HavokData<HKLib.hk2019.UnitTest.TypeVm.Record> 
{
    private static readonly System.Reflection.FieldInfo _floatsInfo = typeof(HKLib.hk2019.UnitTest.TypeVm.Record).GetField("m_floats")!;
    public UnitTestTypeVmRecordData(HavokType type, HKLib.hk2019.UnitTest.TypeVm.Record instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_int":
            case "int":
            {
                if (instance.m_int is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_floats":
            case "floats":
            {
                if (instance.m_floats is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_embedded":
            case "embedded":
            {
                if (instance.m_embedded is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_array":
            case "array":
            {
                if (instance.m_array is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_string":
            case "string":
            {
                if (instance.m_string is null)
                {
                    return true;
                }
                if (instance.m_string is TGet castValue)
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
            case "m_int":
            case "int":
            {
                if (value is not int castValue) return false;
                instance.m_int = castValue;
                return true;
            }
            case "m_floats":
            case "floats":
            {
                if (value is not float[] castValue || castValue.Length != 2) return false;
                try
                {
                    _floatsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_embedded":
            case "embedded":
            {
                if (value is not HKLib.hk2019.UnitTest.TypeVm.Bar castValue) return false;
                instance.m_embedded = castValue;
                return true;
            }
            case "m_array":
            case "array":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_array = castValue;
                return true;
            }
            case "m_string":
            case "string":
            {
                if (value is null)
                {
                    instance.m_string = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_string = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
