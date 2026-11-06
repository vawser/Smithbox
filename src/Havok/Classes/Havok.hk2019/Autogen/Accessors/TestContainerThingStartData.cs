// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class TestContainerThingStartData<T> : Havok.Formats.HavokFieldAccess<Havok.hk2019.TestContainerThingStart<T>>
{
    public TestContainerThingStartData(Havok.hk2019.TestContainerThingStart<T> instance) : base(instance) { }

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
            case "m_data2":
            case "data2":
            {
                value = instance.m_data2;
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
