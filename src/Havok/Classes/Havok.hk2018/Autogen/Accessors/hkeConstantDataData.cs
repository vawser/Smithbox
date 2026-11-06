// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkeConstantDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hke.ConstantData>
{
    public hkeConstantDataData(Havok.hk2018.hke.ConstantData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_value":
            case "value":
            {
                value = instance.m_value;
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
            case "m_value":
            case "value":
            {
                if (value is Havok.hk2018.hke.ConstTypes.Enum castValue)
                {
                    instance.m_value = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_value = (Havok.hk2018.hke.ConstTypes.Enum)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
