// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class UnitTestSimpleRelArrayData : Havok.Formats.HavokFieldAccess<Havok.hk2019.UnitTest.SimpleRelArray>
{
    public UnitTestSimpleRelArrayData(Havok.hk2019.UnitTest.SimpleRelArray instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_intRelArray":
            case "intRelArray":
            {
                value = instance.m_intRelArray;
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
            case "m_intRelArray":
            case "intRelArray":
            {
                if (value is not List<int> castValue) return false;
                instance.m_intRelArray = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
