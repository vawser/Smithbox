// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class TestContainerThingEndData<T> : Havok.Formats.HavokFieldAccess<Havok.hk2018.TestContainerThingEnd<T>>
{
    public TestContainerThingEndData(Havok.hk2018.TestContainerThingEnd<T> instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            case "m_data3":
            case "data3":
            {
                value = instance.m_data3;
                return true;
            }
            case "m_index":
            case "index":
            {
                value = instance.m_index;
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
