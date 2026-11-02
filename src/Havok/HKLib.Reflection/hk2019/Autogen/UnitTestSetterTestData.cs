// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestSetterTestData : HavokData<HKLib.hk2019.UnitTest.SetterTest> 
{
    public UnitTestSetterTestData(HavokType type, HKLib.hk2019.UnitTest.SetterTest instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_intSet":
            case "intSet":
            {
                if (instance.m_intSet is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_int":
            case "int":
            {
                if (instance.m_int is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stringSet":
            case "stringSet":
            {
                if (instance.m_stringSet is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_string":
            case "string":
            {
                if (instance.m_string is null)
                {
                    return true;
                }
                if (instance.m_string is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_pointerSet":
            case "pointerSet":
            {
                if (instance.m_pointerSet is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pointer":
            case "pointer":
            {
                if (instance.m_pointer is null)
                {
                    return true;
                }
                if (instance.m_pointer is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_nestedSet":
            case "nestedSet":
            {
                if (instance.m_nestedSet is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nested":
            case "nested":
            {
                if (instance.m_nested is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_arraySet":
            case "arraySet":
            {
                if (instance.m_arraySet is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_array":
            case "array":
            {
                if (instance.m_array is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_arrayRecSet":
            case "arrayRecSet":
            {
                if (instance.m_arrayRecSet is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_arrayRec":
            case "arrayRec":
            {
                if (instance.m_arrayRec is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_arrayArraySet":
            case "arrayArraySet":
            {
                if (instance.m_arrayArraySet is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_arrayArray":
            case "arrayArray":
            {
                if (instance.m_arrayArray is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is HKLib.hk2019.UnitTest.SetterTest.Foo castValue)
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
                if (value is not HKLib.hk2019.UnitTest.SetterTest.Nested castValue) return false;
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
                if (value is not List<HKLib.hk2019.UnitTest.SetterTest.Nested> castValue) return false;
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
