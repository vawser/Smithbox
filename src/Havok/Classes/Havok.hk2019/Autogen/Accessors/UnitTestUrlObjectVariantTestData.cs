// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class UnitTestUrlObjectVariantTestData : Havok.Formats.HavokFieldAccess<Havok.hk2019.UnitTest.UrlObject.VariantTest>
{
    public UnitTestUrlObjectVariantTestData(Havok.hk2019.UnitTest.UrlObject.VariantTest instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_array1":
            case "array1":
            {
                value = instance.m_array1;
                return true;
            }
            case "m_a1":
            case "a1":
            {
                value = instance.m_a1;
                return true;
            }
            case "m_array2":
            case "array2":
            {
                value = instance.m_array2;
                return true;
            }
            case "m_a4":
            case "a4":
            {
                value = instance.m_a4;
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
            case "m_array1":
            case "array1":
            {
                if (value is not List<object> castValue) return false;
                instance.m_array1 = castValue;
                return true;
            }
            case "m_a1":
            case "a1":
            {
                if (value is not int castValue) return false;
                instance.m_a1 = castValue;
                return true;
            }
            case "m_array2":
            case "array2":
            {
                if (value is not List<object> castValue) return false;
                instance.m_array2 = castValue;
                return true;
            }
            case "m_a4":
            case "a4":
            {
                if (value is not float castValue) return false;
                instance.m_a4 = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
