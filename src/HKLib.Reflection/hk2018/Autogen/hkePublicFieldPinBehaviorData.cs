// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hke;
using HKLib.hk2018.hke.PublicFieldPin;
using Enum = HKLib.hk2018.hke.PublicFieldPin.Enum;

namespace HKLib.Reflection.hk2018;

internal class hkePublicFieldPinBehaviorData : HavokData<PublicFieldPinBehavior> 
{
    public hkePublicFieldPinBehaviorData(HavokType type, PublicFieldPinBehavior instance) : base(type, instance) {}

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
                if (value is Enum castValue)
                {
                    instance.m_value = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_value = (Enum)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
