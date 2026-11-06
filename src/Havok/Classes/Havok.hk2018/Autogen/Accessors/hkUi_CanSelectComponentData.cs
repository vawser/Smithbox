// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkUi_CanSelectComponentData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hk.Ui_CanSelectComponent>
{
    public hkUi_CanSelectComponentData(Havok.hk2018.hk.Ui_CanSelectComponent instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_canSelectComponentFunc":
            case "canSelectComponentFunc":
            {
                value = instance.m_canSelectComponentFunc;
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
            case "m_canSelectComponentFunc":
            case "canSelectComponentFunc":
            {
                if (value is null)
                {
                    instance.m_canSelectComponentFunc = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_canSelectComponentFunc = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
