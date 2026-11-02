// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class AfterReflectNewTestObjData : HavokData<HKLib.hk2019.AfterReflectNewTestObj> 
{
    public AfterReflectNewTestObjData(HavokType type, HKLib.hk2019.AfterReflectNewTestObj instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_afterReflectNewCalled":
            case "afterReflectNewCalled":
            {
                if (instance.m_afterReflectNewCalled is not TGet castValue) return false;
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
            case "m_afterReflectNewCalled":
            case "afterReflectNewCalled":
            {
                if (value is not bool castValue) return false;
                instance.m_afterReflectNewCalled = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
