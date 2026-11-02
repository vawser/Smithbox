// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestUrlObjectShapeData : HavokData<HKLib.hk2019.UnitTest.UrlObject.Shape> 
{
    public UnitTestUrlObjectShapeData(HavokType type, HKLib.hk2019.UnitTest.UrlObject.Shape instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_size":
            case "size":
            {
                if (instance.m_size is not TGet castValue) return false;
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
            case "m_size":
            case "size":
            {
                if (value is not float castValue) return false;
                instance.m_size = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
