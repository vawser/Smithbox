// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class UnitTestSetterTestTwiceNestedData : Havok.Formats.HavokFieldAccess<Havok.hk2018.UnitTest.SetterTest.TwiceNested>
{
    public UnitTestSetterTestTwiceNestedData(Havok.hk2018.UnitTest.SetterTest.TwiceNested instance) : base(instance) { }

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
            default:
            return false;
        }
    }

}
