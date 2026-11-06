// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class UnitTestUrlObjectTestGraphData : Havok.Formats.HavokFieldAccess<Havok.hk2018.UnitTest.UrlObject.TestGraph>
{
    public UnitTestUrlObjectTestGraphData(Havok.hk2018.UnitTest.UrlObject.TestGraph instance) : base(instance) { }

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
            case "m_id":
            case "id":
            {
                value = instance.m_id;
                return true;
            }
            case "m_childA":
            case "childA":
            {
                value = instance.m_childA;
                return true;
            }
            case "m_childB":
            case "childB":
            {
                value = instance.m_childB;
                return true;
            }
            case "m_childC":
            case "childC":
            {
                value = instance.m_childC;
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
            case "m_id":
            case "id":
            {
                if (value is not int castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            case "m_childA":
            case "childA":
            {
                if (value is null)
                {
                    instance.m_childA = default;
                    return true;
                }
                if (value is Havok.hk2018.UnitTest.UrlObject.TestGraph castValue)
                {
                    instance.m_childA = castValue;
                    return true;
                }
                return false;
            }
            case "m_childB":
            case "childB":
            {
                if (value is null)
                {
                    instance.m_childB = default;
                    return true;
                }
                if (value is Havok.hk2018.UnitTest.UrlObject.TestGraph castValue)
                {
                    instance.m_childB = castValue;
                    return true;
                }
                return false;
            }
            case "m_childC":
            case "childC":
            {
                if (value is null)
                {
                    instance.m_childC = default;
                    return true;
                }
                if (value is Havok.hk2018.UnitTest.UrlObject.TestGraph castValue)
                {
                    instance.m_childC = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
