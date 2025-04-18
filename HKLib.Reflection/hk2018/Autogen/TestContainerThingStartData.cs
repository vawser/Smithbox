// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class TestContainerThingStartData<T> : HavokData<TestContainerThingStart<T>> 
{
    public TestContainerThingStartData(HavokType type, TestContainerThingStart<T> instance) : base(type, instance) {}

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
            case "m_data2":
            case "data2":
            {
                if (instance.m_data2 is null)
                {
                    return true;
                }
                if (instance.m_data2 is TGet castValue)
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
            case "m_data2":
            case "data2":
            {
                if (value is null)
                {
                    instance.m_data2 = default;
                    return true;
                }
                if (value is T castValue)
                {
                    instance.m_data2 = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
