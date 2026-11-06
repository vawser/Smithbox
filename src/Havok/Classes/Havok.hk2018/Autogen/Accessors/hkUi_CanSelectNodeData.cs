// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkUi_CanSelectNodeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hk.Ui_CanSelectNode>
{
    public hkUi_CanSelectNodeData(Havok.hk2018.hk.Ui_CanSelectNode instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_canSelectNodeFunc":
            case "canSelectNodeFunc":
            {
                value = instance.m_canSelectNodeFunc;
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
            case "m_canSelectNodeFunc":
            case "canSelectNodeFunc":
            {
                if (value is null)
                {
                    instance.m_canSelectNodeFunc = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_canSelectNodeFunc = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
