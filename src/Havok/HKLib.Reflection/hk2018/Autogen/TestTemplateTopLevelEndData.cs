// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class TestTemplateTopLevelEndData : HavokData<TestTemplateTopLevelEnd> 
{
    public TestTemplateTopLevelEndData(HavokType type, TestTemplateTopLevelEnd instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_intContainer":
            case "intContainer":
            {
                if (instance.m_intContainer is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_realContainer":
            case "realContainer":
            {
                if (instance.m_realContainer is not TGet castValue) return false;
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
            case "m_intContainer":
            case "intContainer":
            {
                if (value is not TestContainerThingEnd<int> castValue) return false;
                instance.m_intContainer = castValue;
                return true;
            }
            case "m_realContainer":
            case "realContainer":
            {
                if (value is not TestContainerThingEnd<float> castValue) return false;
                instance.m_realContainer = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
