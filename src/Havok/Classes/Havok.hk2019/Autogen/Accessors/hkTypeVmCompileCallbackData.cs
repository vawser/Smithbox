// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkTypeVmCompileCallbackData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hk.TypeVmCompileCallback>
{
    public hkTypeVmCompileCallbackData(Havok.hk2019.hk.TypeVmCompileCallback instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_compileFunc":
            case "compileFunc":
            {
                value = instance.m_compileFunc;
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
