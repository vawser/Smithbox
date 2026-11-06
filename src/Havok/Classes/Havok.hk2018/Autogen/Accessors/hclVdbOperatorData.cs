// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclVdbOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclVdbOperator>
{
    public hclVdbOperatorData(Havok.hk2018.hclVdbOperator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_operatorId":
            case "operatorId":
            {
                value = instance.m_operatorId;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_type":
            case "type":
            {
                if (value is null)
                {
                    instance.m_type = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                return false;
            }
            case "m_operatorId":
            case "operatorId":
            {
                if (value is not uint castValue) return false;
                instance.m_operatorId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
