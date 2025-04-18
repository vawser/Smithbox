// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class AfterReflectNewTestObjData : HavokData<AfterReflectNewTestObj> 
{
    public AfterReflectNewTestObjData(HavokType type, AfterReflectNewTestObj instance) : base(type, instance) {}

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
