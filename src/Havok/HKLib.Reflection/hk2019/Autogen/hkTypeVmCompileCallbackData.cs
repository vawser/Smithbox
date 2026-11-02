// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkTypeVmCompileCallbackData : HavokData<HKLib.hk2019.hk.TypeVmCompileCallback> 
{
    public hkTypeVmCompileCallbackData(HavokType type, HKLib.hk2019.hk.TypeVmCompileCallback instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_compileFunc":
            case "compileFunc":
            {
                if (instance.m_compileFunc is null)
                {
                    return true;
                }
                if (instance.m_compileFunc is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_compileFunc":
            case "compileFunc":
            {
                if (value is null)
                {
                    instance.m_compileFunc = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_compileFunc = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
