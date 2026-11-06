// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class TestTemplateTopLevelStartData : Havok.Formats.HavokFieldAccess<Havok.hk2018.TestTemplateTopLevelStart>
{
    public TestTemplateTopLevelStartData(Havok.hk2018.TestTemplateTopLevelStart instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_removedIntContainer":
            case "removedIntContainer":
            {
                value = instance.m_removedIntContainer;
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
            case "m_removedIntContainer":
            case "removedIntContainer":
            {
                if (value is not Havok.hk2018.TestContainerThingStart<int> castValue) return false;
                instance.m_removedIntContainer = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
