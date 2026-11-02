// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestUrlObjectBodyData : HavokData<HKLib.hk2019.UnitTest.UrlObject.Body> 
{
    public UnitTestUrlObjectBodyData(HavokType type, HKLib.hk2019.UnitTest.UrlObject.Body instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_shape":
            case "shape":
            {
                if (instance.m_shape is null)
                {
                    return true;
                }
                if (instance.m_shape is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_child":
            case "child":
            {
                if (instance.m_child is null)
                {
                    return true;
                }
                if (instance.m_child is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is HKLib.hk2019.UnitTest.UrlObject.Shape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_child":
            case "child":
            {
                if (value is null)
                {
                    instance.m_child = default;
                    return true;
                }
                if (value is HKLib.hk2019.UnitTest.UrlObject.Body castValue)
                {
                    instance.m_child = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
