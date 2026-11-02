// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkValueAttributeData<T> : HavokData<HKLib.hk2019.hk.ValueAttribute<T>> 
{
    public hkValueAttributeData(HavokType type, HKLib.hk2019.hk.ValueAttribute<T> instance) : base(type, instance) {}

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
                if (value is T castValue)
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
