// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxSparselyAnimatedStringData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxSparselyAnimatedString>
{
    public hkxSparselyAnimatedStringData(Havok.hk2019.hkxSparselyAnimatedString instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_strings":
            case "strings":
            {
                value = instance.m_strings;
                return true;
            }
            case "m_times":
            case "times":
            {
                value = instance.m_times;
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
            case "m_strings":
            case "strings":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_strings = castValue;
                return true;
            }
            case "m_times":
            case "times":
            {
                if (value is not List<float> castValue) return false;
                instance.m_times = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
