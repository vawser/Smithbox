// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkResultData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkResult>
{
    public hkResultData(Havok.hk2018.hkResult instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_code":
            case "code":
            {
                value = instance.m_code;
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
            case "m_code":
            case "code":
            {
                if (value is not int castValue) return false;
                instance.m_code = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
