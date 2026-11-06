// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkUi_TypeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hk.Ui_Type>
{
    public hkUi_TypeData(Havok.hk2018.hk.Ui_Type instance) : base(instance) { }

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
                if (value is Havok.hk2018.hk.UiTypeEnum castValue)
                {
                    instance.m_value = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_value = (Havok.hk2018.hk.UiTypeEnum)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
