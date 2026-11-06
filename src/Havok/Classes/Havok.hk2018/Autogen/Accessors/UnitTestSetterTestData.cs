// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class UnitTestSetterTestData : Havok.Formats.HavokFieldAccess<Havok.hk2018.UnitTest.SetterTest>
{
    public UnitTestSetterTestData(Havok.hk2018.UnitTest.SetterTest instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_intSet":
            case "intSet":
            {
                value = instance.m_intSet;
                return true;
            }
            case "m_int":
            case "int":
            {
                value = instance.m_int;
                return true;
            }
            case "m_stringSet":
            case "stringSet":
            {
                value = instance.m_stringSet;
                return true;
            }
            case "m_string":
            case "string":
            {
                value = instance.m_string;
                return true;
            }
            case "m_pointerSet":
            case "pointerSet":
            {
                value = instance.m_pointerSet;
                return true;
            }
            case "m_pointer":
            case "pointer":
            {
                value = instance.m_pointer;
                return true;
            }
            case "m_nestedSet":
            case "nestedSet":
            {
                value = instance.m_nestedSet;
                return true;
            }
            case "m_nested":
            case "nested":
            {
                value = instance.m_nested;
                return true;
            }
            case "m_arraySet":
            case "arraySet":
            {
                value = instance.m_arraySet;
                return true;
            }
            case "m_array":
            case "array":
            {
                value = instance.m_array;
                return true;
            }
            case "m_arrayRecSet":
            case "arrayRecSet":
            {
                value = instance.m_arrayRecSet;
                return true;
            }
            case "m_arrayRec":
            case "arrayRec":
            {
                value = instance.m_arrayRec;
                return true;
            }
            case "m_arrayArraySet":
            case "arrayArraySet":
            {
                value = instance.m_arrayArraySet;
                return true;
            }
            case "m_arrayArray":
            case "arrayArray":
            {
                value = instance.m_arrayArray;
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
            case "m_intSet":
            case "intSet":
            {
                if (value is not int castValue) return false;
                instance.m_intSet = castValue;
                return true;
            }
            case "m_int":
            case "int":
            {
                if (value is not int castValue) return false;
                instance.m_int = castValue;
                return true;
            }
            case "m_stringSet":
            case "stringSet":
            {
                if (value is not int castValue) return false;
                instance.m_stringSet = castValue;
                return true;
            }
            case "m_string":
            case "string":
            {
                if (value is null)
                {
                    instance.m_string = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_string = castValue;
                    return true;
                }
                return false;
            }
            case "m_pointerSet":
            case "pointerSet":
            {
                if (value is not int castValue) return false;
                instance.m_pointerSet = castValue;
                return true;
            }
            case "m_pointer":
            case "pointer":
            {
                if (value is null)
                {
                    instance.m_pointer = default;
                    return true;
                }
                if (value is Havok.hk2018.UnitTest.SetterTest.Foo castValue)
                {
                    instance.m_pointer = castValue;
                    return true;
                }
                return false;
            }
            case "m_nestedSet":
            case "nestedSet":
            {
                if (value is not int castValue) return false;
                instance.m_nestedSet = castValue;
                return true;
            }
            case "m_nested":
            case "nested":
            {
                if (value is not Havok.hk2018.UnitTest.SetterTest.Nested castValue) return false;
                instance.m_nested = castValue;
                return true;
            }
            case "m_arraySet":
            case "arraySet":
            {
                if (value is not int castValue) return false;
                instance.m_arraySet = castValue;
                return true;
            }
            case "m_array":
            case "array":
            {
                if (value is not List<int> castValue) return false;
                instance.m_array = castValue;
                return true;
            }
            case "m_arrayRecSet":
            case "arrayRecSet":
            {
                if (value is not int castValue) return false;
                instance.m_arrayRecSet = castValue;
                return true;
            }
            case "m_arrayRec":
            case "arrayRec":
            {
                if (value is not List<Havok.hk2018.UnitTest.SetterTest.Nested> castValue) return false;
                instance.m_arrayRec = castValue;
                return true;
            }
            case "m_arrayArraySet":
            case "arrayArraySet":
            {
                if (value is not int castValue) return false;
                instance.m_arrayArraySet = castValue;
                return true;
            }
            case "m_arrayArray":
            case "arrayArray":
            {
                if (value is not List<List<int>> castValue) return false;
                instance.m_arrayArray = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
