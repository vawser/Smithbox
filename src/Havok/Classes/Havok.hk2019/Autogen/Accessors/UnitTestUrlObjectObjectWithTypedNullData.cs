// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class UnitTestUrlObjectObjectWithTypedNullData : Havok.Formats.HavokFieldAccess<Havok.hk2019.UnitTest.UrlObject.ObjectWithTypedNull>
{
    public UnitTestUrlObjectObjectWithTypedNullData(Havok.hk2019.UnitTest.UrlObject.ObjectWithTypedNull instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_typeRef":
            case "typeRef":
            {
                value = instance.m_typeRef;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
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
            case "m_typeRef":
            case "typeRef":
            {
                if (value is not List<Havok.hk2019.IHavokObject?> castValue) return false;
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
