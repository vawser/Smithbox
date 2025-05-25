// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class TestSimpleContainerExRecordData : HavokData<TestSimpleContainerExRecord> 
{
    private static readonly System.Reflection.FieldInfo _one_recordInfo = typeof(TestSimpleContainerExRecord).GetField("m_one_record")!;
    private static readonly System.Reflection.FieldInfo _one_exRecordInfo = typeof(TestSimpleContainerExRecord).GetField("m_one_exRecord")!;
    private static readonly System.Reflection.FieldInfo _many_recordsInfo = typeof(TestSimpleContainerExRecord).GetField("m_many_records")!;
    private static readonly System.Reflection.FieldInfo _many_exRecordsInfo = typeof(TestSimpleContainerExRecord).GetField("m_many_exRecords")!;
    public TestSimpleContainerExRecordData(HavokType type, TestSimpleContainerExRecord instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pad":
            case "pad":
            {
                if (instance.m_pad is not TGet castValue) return false;
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
            case "m_one_exRecord":
            case "one_exRecord":
            {
                if (instance.m_one_exRecord is not TGet castValue) return false;
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
            case "m_many_exRecords":
            case "many_exRecords":
            {
                if (instance.m_many_exRecords is not TGet castValue) return false;
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
            case "m_pad":
            case "pad":
            {
                if (value is not ulong castValue) return false;
                instance.m_pad = castValue;
                return true;
            }
            case "m_one_record":
            case "one_record":
            {
                if (value is not TestSimpleRecord[] castValue || castValue.Length != 1) return false;
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
            case "m_one_exRecord":
            case "one_exRecord":
            {
                if (value is not TestSimpleExRecord[] castValue || castValue.Length != 1) return false;
                try
                {
                    _one_exRecordInfo.SetValue(instance, value);
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
                if (value is not TestSimpleRecord[] castValue || castValue.Length != 10) return false;
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
            case "m_many_exRecords":
            case "many_exRecords":
            {
                if (value is not TestSimpleExRecord[] castValue || castValue.Length != 10) return false;
                try
                {
                    _many_exRecordsInfo.SetValue(instance, value);
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
