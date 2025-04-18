// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbExpressionDataData : HavokData<hkbExpressionData> 
{
    public hkbExpressionDataData(HavokType type, hkbExpressionData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_expression":
            case "expression":
            {
                if (instance.m_expression is null)
                {
                    return true;
                }
                if (instance.m_expression is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_assignmentVariableIndex":
            case "assignmentVariableIndex":
            {
                if (instance.m_assignmentVariableIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_assignmentEventIndex":
            case "assignmentEventIndex":
            {
                if (instance.m_assignmentEventIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_eventMode":
            case "eventMode":
            {
                if (instance.m_eventMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_eventMode is TGet sbyteValue)
                {
                    value = sbyteValue;
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
                if (value is hkbExpressionData.ExpressionEventMode castValue)
                {
                    instance.m_eventMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_eventMode = (hkbExpressionData.ExpressionEventMode)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
