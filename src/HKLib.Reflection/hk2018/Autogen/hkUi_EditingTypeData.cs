// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hk;

namespace HKLib.Reflection.hk2018;

internal class hkUi_EditingTypeData : HavokData<Ui_EditingType> 
{
    public hkUi_EditingTypeData(HavokType type, Ui_EditingType instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_value":
            case "value":
            {
                if (instance.m_value is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_value is TGet intValue)
                {
                    value = intValue;
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
            case "m_value":
            case "value":
            {
                if (value is EditingTypeEnum castValue)
                {
                    instance.m_value = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_value = (EditingTypeEnum)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
