// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbExpressionDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbExpressionData>
{
    public hkbExpressionDataData(Havok.hk2018.hkbExpressionData instance) : base(instance) { }

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
            case "m_assignmentVariableIndex":
            case "assignmentVariableIndex":
            {
                value = instance.m_assignmentVariableIndex;
                return true;
            }
            case "m_assignmentEventIndex":
            case "assignmentEventIndex":
            {
                value = instance.m_assignmentEventIndex;
                return true;
            }
            case "m_eventMode":
            case "eventMode":
            {
                value = instance.m_eventMode;
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
            case "m_assignmentVariableIndex":
            case "assignmentVariableIndex":
            {
                if (value is not int castValue) return false;
                instance.m_assignmentVariableIndex = castValue;
                return true;
            }
            case "m_assignmentEventIndex":
            case "assignmentEventIndex":
            {
                if (value is not int castValue) return false;
                instance.m_assignmentEventIndex = castValue;
                return true;
            }
            case "m_eventMode":
            case "eventMode":
            {
                if (value is Havok.hk2018.hkbExpressionData.ExpressionEventMode castValue)
                {
                    instance.m_eventMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_eventMode = (Havok.hk2018.hkbExpressionData.ExpressionEventMode)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
