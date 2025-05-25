// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hk;
using Type = HKLib.hk2018.hk.Type;

namespace HKLib.Reflection.hk2018;

internal class hkTypeData : HavokData<Type> 
{
    public hkTypeData(HavokType type, Type instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_value":
            case "value":
            {
                if (instance.m_value is null)
                {
                    return true;
                }
                if (instance.m_value is TGet castValue)
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
            case "m_value":
            case "value":
            {
                if (value is null)
                {
                    instance.m_value = default;
                    return true;
                }
                if (value is IHavokObject castValue)
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
