// Automatically Generated

namespace HKLib.hk2018;

public class hkbCompiledExpressionSet : hkReferencedObject
{
    public List<hkbCompiledExpressionSet.Token> m_rpn = new();

    public List<int> m_expressionToRpnIndex = new();

    public sbyte m_numExpressions;


    public class Token : IHavokObject
    {
        public float m_data;

        public hkbCompiledExpressionSet.Token.TokenType m_type;

        public hkbCompiledExpressionSet.Token.Operator m_operator;


        public enum Operator : int
        {
            OP_NOP = 0,
            OP_RAND01 = 1,
            OP_LOGICAL_NOT = 2,
            OP_UNARY_MINUS = 3,
            OP_UNARY_PLUS = 4,
            OP_SIN = 5,
            OP_COS = 6,
            OP_ASIN = 7,
            OP_ACOS = 8,
            OP_SQRT = 9,
            OP_FABS = 10,
            OP_CEIL = 11,
            OP_FLOOR = 12,
            OP_SQRTINV = 13,
            OP_MUL = 14,
            OP_DIV = 15,
            OP_ADD = 16,
            OP_SUB = 17,
            OP_LOGICAL_OR = 18,
            OP_LOGICAL_AND = 19,
            OP_EQ = 20,
            OP_NEQ = 21,
            OP_LT = 22,
            OP_GT = 23,
            OP_LEQ = 24,
            OP_GEQ = 25,
            OP_POW = 26,
            OP_MAX2 = 27,
            OP_MIN2 = 28,
            OP_RANDRANGE = 29,
            OP_ATAN2 = 30,
            OP_CLAMP = 31,
            OP_MOD = 32,
            OP_DEG2RAD = 33,
            OP_RAD2DEG = 34,
            OP_COSD = 35,
            OP_SIND = 36,
            OP_ACOSD = 37,
            OP_ASIND = 38,
            OP_ATAN2D = 39,
            OP_SIGN = 40,
            OP_LERP = 41,
            OP_CLERP = 42,
            OP_COND = 43
        }

        public enum TokenType : int
        {
            TOKEN_TYPE_NONE = 0,
            TOKEN_TYPE_OPERATOR = 1,
            TOKEN_TYPE_NUMBER = 2,
            TOKEN_TYPE_VARIABLE_INDEX = 3,
            TOKEN_TYPE_OPENING_PAREN = 4,
            TOKEN_TYPE_CLOSING_PAREN = 5,
            TOKEN_TYPE_COMMA = 6,
            TOKEN_TYPE_CHARACTER_PROPERTY_INDEX = 7
        }

    }


}

