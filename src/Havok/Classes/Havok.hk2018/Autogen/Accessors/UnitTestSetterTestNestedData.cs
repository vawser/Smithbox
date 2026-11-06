// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class UnitTestSetterTestNestedData : Havok.Formats.HavokFieldAccess<Havok.hk2018.UnitTest.SetterTest.Nested>
{
    public UnitTestSetterTestNestedData(Havok.hk2018.UnitTest.SetterTest.Nested instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_int":
            case "int":
            {
                value = instance.m_int;
                return true;
            }
            case "m_float":
            case "float":
            {
                value = instance.m_float;
                return true;
            }
            case "m_plain":
            case "plain":
            {
                value = instance.m_plain;
                return true;
            }
            case "m_twiceNestedSet":
            case "twiceNestedSet":
            {
                value = instance.m_twiceNestedSet;
                return true;
            }
            case "m_twiceNested":
            case "twiceNested":
            {
                value = instance.m_twiceNested;
                return true;
            }
            case "m_array":
            case "array":
            {
                value = instance.m_array;
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
            case "m_int":
            case "int":
            {
                if (value is not int castValue) return false;
                instance.m_int = castValue;
                return true;
            }
            case "m_float":
            case "float":
            {
                if (value is not float castValue) return false;
                instance.m_float = castValue;
                return true;
            }
            case "m_plain":
            case "plain":
            {
                if (value is not Havok.hk2018.UnitTest.SetterTest.TwiceNested castValue) return false;
                instance.m_plain = castValue;
                return true;
            }
            case "m_twiceNestedSet":
            case "twiceNestedSet":
            {
                if (value is not int castValue) return false;
                instance.m_twiceNestedSet = castValue;
                return true;
            }
            case "m_twiceNested":
            case "twiceNested":
            {
                if (value is not Havok.hk2018.UnitTest.SetterTest.TwiceNested castValue) return false;
                instance.m_twiceNested = castValue;
                return true;
            }
            case "m_array":
            case "array":
            {
                if (value is not List<int> castValue) return false;
                instance.m_array = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
