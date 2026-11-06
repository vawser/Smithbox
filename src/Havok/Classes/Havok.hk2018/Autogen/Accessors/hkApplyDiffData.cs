// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkApplyDiffData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hk.ApplyDiff>
{
    public hkApplyDiffData(Havok.hk2018.hk.ApplyDiff instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_fn":
            case "fn":
            {
                value = instance.m_fn;
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
            case "m_fn":
            case "fn":
            {
                if (value is null)
                {
                    instance.m_fn = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_fn = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
