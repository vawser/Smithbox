// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class UnitTestUrlObjectShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.UnitTest.UrlObject.Shape>
{
    public UnitTestUrlObjectShapeData(Havok.hk2018.UnitTest.UrlObject.Shape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_size":
            case "size":
            {
                value = instance.m_size;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
