// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCompiledExpressionSetData : HavokData<hkbCompiledExpressionSet> 
{
    public hkbCompiledExpressionSetData(HavokType type, hkbCompiledExpressionSet instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rpn":
            case "rpn":
            {
                if (instance.m_rpn is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_expressionToRpnIndex":
            case "expressionToRpnIndex":
            {
                if (instance.m_expressionToRpnIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numExpressions":
            case "numExpressions":
            {
                if (instance.m_numExpressions is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_rpn":
            case "rpn":
            {
                if (value is not List<hkbCompiledExpressionSet.Token> castValue) return false;
                instance.m_rpn = castValue;
                return true;
            }
            case "m_expressionToRpnIndex":
            case "expressionToRpnIndex":
            {
                if (value is not List<int> castValue) return false;
                instance.m_expressionToRpnIndex = castValue;
                return true;
            }
            case "m_numExpressions":
            case "numExpressions":
            {
                if (value is not sbyte castValue) return false;
                instance.m_numExpressions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
