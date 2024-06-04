// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hk;

namespace HKLib.Reflection.hk2018;

internal class hkPreferredSerializationFormatData : HavokData<PreferredSerializationFormat> 
{
    public hkPreferredSerializationFormatData(HavokType type, PreferredSerializationFormat instance) : base(type, instance) {}

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
                if ((uint)instance.m_value is TGet uintValue)
                {
                    value = uintValue;
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
                if (value is PreferredSerializationFormat.Format castValue)
                {
                    instance.m_value = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_value = (PreferredSerializationFormat.Format)uintValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
