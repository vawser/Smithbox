// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class TestTemplateTopLevelStartData : HavokData<HKLib.hk2019.TestTemplateTopLevelStart> 
{
    public TestTemplateTopLevelStartData(HavokType type, HKLib.hk2019.TestTemplateTopLevelStart instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_removedIntContainer":
            case "removedIntContainer":
            {
                if (instance.m_removedIntContainer is not TGet castValue) return false;
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
            case "m_removedIntContainer":
            case "removedIntContainer":
            {
                if (value is not HKLib.hk2019.TestContainerThingStart<int> castValue) return false;
                instance.m_removedIntContainer = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
