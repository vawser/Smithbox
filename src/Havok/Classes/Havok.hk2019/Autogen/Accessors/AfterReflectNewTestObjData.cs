// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class AfterReflectNewTestObjData : Havok.Formats.HavokFieldAccess<Havok.hk2019.AfterReflectNewTestObj>
{
    public AfterReflectNewTestObjData(Havok.hk2019.AfterReflectNewTestObj instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_afterReflectNewCalled":
            case "afterReflectNewCalled":
            {
                value = instance.m_afterReflectNewCalled;
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
