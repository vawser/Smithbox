// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkUi_EditingTypeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hk.Ui_EditingType>
{
    public hkUi_EditingTypeData(Havok.hk2018.hk.Ui_EditingType instance) : base(instance) { }

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
                if (value is Havok.hk2018.hk.EditingTypeEnum castValue)
                {
                    instance.m_value = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_value = (Havok.hk2018.hk.EditingTypeEnum)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
