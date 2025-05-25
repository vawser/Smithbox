// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hk;

namespace HKLib.Reflection.hk2018;

internal class hkUi_CanSelectNodeData : HavokData<Ui_CanSelectNode> 
{
    public hkUi_CanSelectNodeData(HavokType type, Ui_CanSelectNode instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_canSelectNodeFunc":
            case "canSelectNodeFunc":
            {
                if (instance.m_canSelectNodeFunc is null)
                {
                    return true;
                }
                if (instance.m_canSelectNodeFunc is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
