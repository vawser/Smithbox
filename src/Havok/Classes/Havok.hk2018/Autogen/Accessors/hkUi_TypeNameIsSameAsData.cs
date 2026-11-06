// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkUi_TypeNameIsSameAsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hk.Ui_TypeNameIsSameAs>
{
    public hkUi_TypeNameIsSameAsData(Havok.hk2018.hk.Ui_TypeNameIsSameAs instance) : base(instance) { }

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
                if (value is null)
                {
                    instance.m_value = default;
                    return true;
                }
                if (value is Havok.hk2018.IHavokObject castValue)
                {
                    instance.m_value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
