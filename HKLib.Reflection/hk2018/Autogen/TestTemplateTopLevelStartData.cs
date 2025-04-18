// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class TestTemplateTopLevelStartData : HavokData<TestTemplateTopLevelStart> 
{
    public TestTemplateTopLevelStartData(HavokType type, TestTemplateTopLevelStart instance) : base(type, instance) {}

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
                if (value is not TestContainerThingStart<int> castValue) return false;
                instance.m_removedIntContainer = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
