// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestUrlObjectObjectWithTypedNullData : HavokData<HKLib.hk2019.UnitTest.UrlObject.ObjectWithTypedNull> 
{
    public UnitTestUrlObjectObjectWithTypedNullData(HavokType type, HKLib.hk2019.UnitTest.UrlObject.ObjectWithTypedNull instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_typeRef":
            case "typeRef":
            {
                if (instance.m_typeRef is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
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
            case "m_typeRef":
            case "typeRef":
            {
                if (value is not List<HKLib.hk2019.IHavokObject?> castValue) return false;
                instance.m_typeRef = castValue;
                return true;
            }
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
            default:
            return false;
        }
    }

}
