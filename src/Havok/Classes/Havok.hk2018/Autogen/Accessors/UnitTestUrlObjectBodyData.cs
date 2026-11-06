// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class UnitTestUrlObjectBodyData : Havok.Formats.HavokFieldAccess<Havok.hk2018.UnitTest.UrlObject.Body>
{
    public UnitTestUrlObjectBodyData(Havok.hk2018.UnitTest.UrlObject.Body instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_shape":
            case "shape":
            {
                value = instance.m_shape;
                return true;
            }
            case "m_child":
            case "child":
            {
                value = instance.m_child;
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
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is Havok.hk2018.UnitTest.UrlObject.Shape castValue)
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
                if (value is Havok.hk2018.UnitTest.UrlObject.Body castValue)
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
