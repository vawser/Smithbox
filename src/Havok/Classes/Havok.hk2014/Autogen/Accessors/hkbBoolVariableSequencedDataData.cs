// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbBoolVariableSequencedDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbBoolVariableSequencedData>
{
    public hkbBoolVariableSequencedDataData(Havok.hk2014.hkbBoolVariableSequencedData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_samples":
            case "samples":
            {
                value = instance.m_samples;
                return true;
            }
            case "m_variableIndex":
            case "variableIndex":
            {
                value = instance.m_variableIndex;
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
            case "m_samples":
            case "samples":
            {
                if (value is not List<Havok.hk2014.hkbBoolVariableSequencedData.Sample> castValue) return false;
                instance.m_samples = castValue;
                return true;
            }
            case "m_variableIndex":
            case "variableIndex":
            {
                if (value is not int castValue) return false;
                instance.m_variableIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
