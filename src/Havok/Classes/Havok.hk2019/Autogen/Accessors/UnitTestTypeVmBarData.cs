// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class UnitTestTypeVmBarData : Havok.Formats.HavokFieldAccess<Havok.hk2019.UnitTest.TypeVm.Bar>
{
    private static readonly System.Reflection.FieldInfo _floatsInfo = typeof(Havok.hk2019.UnitTest.TypeVm.Bar).GetField("m_floats")!;
    public UnitTestTypeVmBarData(Havok.hk2019.UnitTest.TypeVm.Bar instance) : base(instance) { }

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
            case "m_floats":
            case "floats":
            {
                value = instance.m_floats;
                return true;
            }
            case "m_embedded":
            case "embedded":
            {
                value = instance.m_embedded;
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
            case "m_floats":
            case "floats":
            {
                if (value is not float[] castValue || castValue.Length != 2) return false;
                try
                {
                    _floatsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_embedded":
            case "embedded":
            {
                if (value is not Havok.hk2019.UnitTest.TypeVm.Foo castValue) return false;
                instance.m_embedded = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
