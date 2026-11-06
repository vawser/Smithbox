// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclVdbClothStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclVdbClothState>
{
    public hclVdbClothStateData(Havok.hk2018.hclVdbClothState instance) : base(instance) { }

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
            case "m_stateId":
            case "stateId":
            {
                value = instance.m_stateId;
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
            case "m_stateId":
            case "stateId":
            {
                if (value is not uint castValue) return false;
                instance.m_stateId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
