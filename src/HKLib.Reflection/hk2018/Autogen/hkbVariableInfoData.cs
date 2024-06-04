// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbVariableInfoData : HavokData<hkbVariableInfo> 
{
    public hkbVariableInfoData(HavokType type, hkbVariableInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_role":
            case "role":
            {
                if (instance.m_role is not TGet castValue) return false;
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
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_role":
            case "role":
            {
                if (value is not hkbRoleAttribute castValue) return false;
                instance.m_role = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is hkbVariableInfo.VariableType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (hkbVariableInfo.VariableType)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
