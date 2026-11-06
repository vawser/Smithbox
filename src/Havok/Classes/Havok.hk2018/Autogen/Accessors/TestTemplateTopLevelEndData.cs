// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class TestTemplateTopLevelEndData : Havok.Formats.HavokFieldAccess<Havok.hk2018.TestTemplateTopLevelEnd>
{
    public TestTemplateTopLevelEndData(Havok.hk2018.TestTemplateTopLevelEnd instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_intContainer":
            case "intContainer":
            {
                value = instance.m_intContainer;
                return true;
            }
            case "m_realContainer":
            case "realContainer":
            {
                value = instance.m_realContainer;
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
            case "m_intContainer":
            case "intContainer":
            {
                if (value is not Havok.hk2018.TestContainerThingEnd<int> castValue) return false;
                instance.m_intContainer = castValue;
                return true;
            }
            case "m_realContainer":
            case "realContainer":
            {
                if (value is not Havok.hk2018.TestContainerThingEnd<float> castValue) return false;
                instance.m_realContainer = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
