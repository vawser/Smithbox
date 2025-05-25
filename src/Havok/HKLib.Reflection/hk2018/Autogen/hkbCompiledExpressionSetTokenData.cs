// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCompiledExpressionSetTokenData : HavokData<hkbCompiledExpressionSet.Token> 
{
    public hkbCompiledExpressionSetTokenData(HavokType type, hkbCompiledExpressionSet.Token instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_data":
            case "data":
            {
                if (instance.m_data is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_type is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_operator":
            case "operator":
            {
                if (instance.m_operator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_operator is TGet sbyteValue)
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
            case "m_data":
            case "data":
            {
                if (value is not float castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is hkbCompiledExpressionSet.Token.TokenType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (hkbCompiledExpressionSet.Token.TokenType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_operator":
            case "operator":
            {
                if (value is hkbCompiledExpressionSet.Token.Operator castValue)
                {
                    instance.m_operator = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_operator = (hkbCompiledExpressionSet.Token.Operator)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
