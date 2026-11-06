// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkTypeLayoutData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hk.TypeLayout>
{
    public hkTypeLayoutData(Havok.hk2019.hk.TypeLayout instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_func":
            case "func":
            {
                value = instance.m_func;
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
            case "m_func":
            case "func":
            {
                if (value is null)
                {
                    instance.m_func = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_func = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
