// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbStringConditionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbStringCondition>
{
    public hkbStringConditionData(Havok.hk2014.hkbStringCondition instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_conditionString":
            case "conditionString":
            {
                value = instance.m_conditionString;
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
            case "m_conditionString":
            case "conditionString":
            {
                if (value is null)
                {
                    instance.m_conditionString = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_conditionString = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
