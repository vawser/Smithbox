// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbRealVariableSequencedDataSampleData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbRealVariableSequencedData.Sample>
{
    public hkbRealVariableSequencedDataSampleData(Havok.hk2018.hkbRealVariableSequencedData.Sample instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_time":
            case "time":
            {
                value = instance.m_time;
                return true;
            }
            case "m_value":
            case "value":
            {
                value = instance.m_value;
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
            case "m_time":
            case "time":
            {
                if (value is not float castValue) return false;
                instance.m_time = castValue;
                return true;
            }
            case "m_value":
            case "value":
            {
                if (value is not float castValue) return false;
                instance.m_value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
