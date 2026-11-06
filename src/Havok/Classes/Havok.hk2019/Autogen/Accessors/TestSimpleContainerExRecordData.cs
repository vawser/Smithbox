// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class TestSimpleContainerExRecordData : Havok.Formats.HavokFieldAccess<Havok.hk2019.TestSimpleContainerExRecord>
{
    private static readonly System.Reflection.FieldInfo _one_recordInfo = typeof(Havok.hk2019.TestSimpleContainerExRecord).GetField("m_one_record")!;
    private static readonly System.Reflection.FieldInfo _one_exRecordInfo = typeof(Havok.hk2019.TestSimpleContainerExRecord).GetField("m_one_exRecord")!;
    private static readonly System.Reflection.FieldInfo _many_recordsInfo = typeof(Havok.hk2019.TestSimpleContainerExRecord).GetField("m_many_records")!;
    private static readonly System.Reflection.FieldInfo _many_exRecordsInfo = typeof(Havok.hk2019.TestSimpleContainerExRecord).GetField("m_many_exRecords")!;
    public TestSimpleContainerExRecordData(Havok.hk2019.TestSimpleContainerExRecord instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pad":
            case "pad":
            {
                value = instance.m_pad;
                return true;
            }
            case "m_one_record":
            case "one_record":
            {
                value = instance.m_one_record;
                return true;
            }
            case "m_one_exRecord":
            case "one_exRecord":
            {
                value = instance.m_one_exRecord;
                return true;
            }
            case "m_many_records":
            case "many_records":
            {
                value = instance.m_many_records;
                return true;
            }
            case "m_many_exRecords":
            case "many_exRecords":
            {
                value = instance.m_many_exRecords;
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
                if (value is not Havok.hk2019.TestSimpleRecord[] castValue || castValue.Length != 1) return false;
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
                if (value is not Havok.hk2019.TestSimpleExRecord[] castValue || castValue.Length != 1) return false;
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
                if (value is not Havok.hk2019.TestSimpleRecord[] castValue || castValue.Length != 10) return false;
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
                if (value is not Havok.hk2019.TestSimpleExRecord[] castValue || castValue.Length != 10) return false;
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
