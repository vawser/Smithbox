// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestTypeVmBarData : HavokData<HKLib.hk2019.UnitTest.TypeVm.Bar> 
{
    private static readonly System.Reflection.FieldInfo _floatsInfo = typeof(HKLib.hk2019.UnitTest.TypeVm.Bar).GetField("m_floats")!;
    public UnitTestTypeVmBarData(HavokType type, HKLib.hk2019.UnitTest.TypeVm.Bar instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_int":
            case "int":
            {
                if (instance.m_int is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_floats":
            case "floats":
            {
                if (instance.m_floats is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_embedded":
            case "embedded":
            {
                if (instance.m_embedded is not TGet castValue) return false;
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
                if (value is not HKLib.hk2019.UnitTest.TypeVm.Foo castValue) return false;
                instance.m_embedded = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
