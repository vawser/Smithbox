// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkUi_GroupData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hk.Ui_Group>
{
    public hkUi_GroupData(Havok.hk2018.hk.Ui_Group instance) : base(instance) { }

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
            case "m_order":
            case "order":
            {
                value = instance.m_order;
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
            case "m_order":
            case "order":
            {
                if (value is not float castValue) return false;
                instance.m_order = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
