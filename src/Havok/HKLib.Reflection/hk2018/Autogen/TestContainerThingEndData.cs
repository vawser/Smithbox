// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class TestContainerThingEndData<T> : HavokData<TestContainerThingEnd<T>> 
{
    public TestContainerThingEndData(HavokType type, TestContainerThingEnd<T> instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_data":
            case "data":
            {
                if (instance.m_data is null)
                {
                    return true;
                }
                if (instance.m_data is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_data3":
            case "data3":
            {
                if (instance.m_data3 is null)
                {
                    return true;
                }
                if (instance.m_data3 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_index":
            case "index":
            {
                if (instance.m_index is not TGet castValue) return false;
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
            case "m_data":
            case "data":
            {
                if (value is null)
                {
                    instance.m_data = default;
                    return true;
                }
                if (value is T castValue)
                {
                    instance.m_data = castValue;
                    return true;
                }
                return false;
            }
            case "m_data3":
            case "data3":
            {
                if (value is null)
                {
                    instance.m_data3 = default;
                    return true;
                }
                if (value is T castValue)
                {
                    instance.m_data3 = castValue;
                    return true;
                }
                return false;
            }
            case "m_index":
            case "index":
            {
                if (value is not int castValue) return false;
                instance.m_index = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
