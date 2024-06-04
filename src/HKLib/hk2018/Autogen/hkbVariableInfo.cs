// Automatically Generated

namespace HKLib.hk2018;

public class hkbVariableInfo : IHavokObject
{
    public hkbRoleAttribute m_role = new();

    public hkbVariableInfo.VariableType m_type;


    public enum VariableType : int
    {
        VARIABLE_TYPE_INVALID = -1,
        VARIABLE_TYPE_BOOL = 0,
        VARIABLE_TYPE_INT8 = 1,
        VARIABLE_TYPE_INT16 = 2,
        VARIABLE_TYPE_INT32 = 3,
        VARIABLE_TYPE_REAL = 4,
        VARIABLE_TYPE_POINTER = 5,
        VARIABLE_TYPE_VECTOR3 = 6,
        VARIABLE_TYPE_VECTOR4 = 7,
        VARIABLE_TYPE_QUATERNION = 8
    }

}

