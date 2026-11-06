// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbVariableInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbVariableInfo>
{
    public hkbVariableInfoData(Havok.hk2018.hkbVariableInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_role":
            case "role":
            {
                value = instance.m_role;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
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
            case "m_role":
            case "role":
            {
                if (value is not Havok.hk2018.hkbRoleAttribute castValue) return false;
                instance.m_role = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2018.hkbVariableInfo.VariableType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (Havok.hk2018.hkbVariableInfo.VariableType)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
