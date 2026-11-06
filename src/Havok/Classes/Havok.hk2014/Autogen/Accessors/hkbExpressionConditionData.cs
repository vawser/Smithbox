// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbExpressionConditionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbExpressionCondition>
{
    public hkbExpressionConditionData(Havok.hk2014.hkbExpressionCondition instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_expression":
            case "expression":
            {
                value = instance.m_expression;
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
            case "m_expression":
            case "expression":
            {
                if (value is null)
                {
                    instance.m_expression = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_expression = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
