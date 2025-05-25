// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hk;

namespace HKLib.Reflection.hk2018;

internal class hkUi_CanSelectComponentData : HavokData<Ui_CanSelectComponent> 
{
    public hkUi_CanSelectComponentData(HavokType type, Ui_CanSelectComponent instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_canSelectComponentFunc":
            case "canSelectComponentFunc":
            {
                if (instance.m_canSelectComponentFunc is null)
                {
                    return true;
                }
                if (instance.m_canSelectComponentFunc is TGet castValue)
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
