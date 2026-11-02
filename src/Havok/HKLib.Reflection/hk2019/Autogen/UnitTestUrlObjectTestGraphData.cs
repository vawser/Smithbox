// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestUrlObjectTestGraphData : HavokData<HKLib.hk2019.UnitTest.UrlObject.TestGraph> 
{
    public UnitTestUrlObjectTestGraphData(HavokType type, HKLib.hk2019.UnitTest.UrlObject.TestGraph instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                if (instance.m_id is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_childA":
            case "childA":
            {
                if (instance.m_childA is null)
                {
                    return true;
                }
                if (instance.m_childA is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_childB":
            case "childB":
            {
                if (instance.m_childB is null)
                {
                    return true;
                }
                if (instance.m_childB is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_childC":
            case "childC":
            {
                if (instance.m_childC is null)
                {
                    return true;
                }
                if (instance.m_childC is TGet castValue)
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
                if (value is HKLib.hk2019.UnitTest.UrlObject.TestGraph castValue)
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
                if (value is HKLib.hk2019.UnitTest.UrlObject.TestGraph castValue)
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
                if (value is HKLib.hk2019.UnitTest.UrlObject.TestGraph castValue)
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
