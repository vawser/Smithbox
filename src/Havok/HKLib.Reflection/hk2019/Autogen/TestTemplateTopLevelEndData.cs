// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class TestTemplateTopLevelEndData : HavokData<HKLib.hk2019.TestTemplateTopLevelEnd> 
{
    public TestTemplateTopLevelEndData(HavokType type, HKLib.hk2019.TestTemplateTopLevelEnd instance) : base(type, instance) {}

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
                if (value is not HKLib.hk2019.TestContainerThingEnd<int> castValue) return false;
                instance.m_intContainer = castValue;
                return true;
            }
            case "m_realContainer":
            case "realContainer":
            {
                if (value is not HKLib.hk2019.TestContainerThingEnd<float> castValue) return false;
                instance.m_realContainer = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
