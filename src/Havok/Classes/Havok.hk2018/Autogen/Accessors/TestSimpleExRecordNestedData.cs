// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class TestSimpleExRecordNestedData : Havok.Formats.HavokFieldAccess<Havok.hk2018.TestSimpleExRecord.Nested>
{
    public TestSimpleExRecordNestedData(Havok.hk2018.TestSimpleExRecord.Nested instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
