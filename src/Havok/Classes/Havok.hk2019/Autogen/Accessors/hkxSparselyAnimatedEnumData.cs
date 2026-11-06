// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxSparselyAnimatedEnumData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxSparselyAnimatedEnum>
{
    public hkxSparselyAnimatedEnumData(Havok.hk2019.hkxSparselyAnimatedEnum instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_ints":
            case "ints":
            {
                value = instance.m_ints;
                return true;
            }
            case "m_times":
            case "times":
            {
                value = instance.m_times;
                return true;
            }
            case "m_enum":
            case "enum":
            {
                value = instance.m_enum;
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
            case "m_ints":
            case "ints":
            {
                if (value is not List<int> castValue) return false;
                instance.m_ints = castValue;
                return true;
            }
            case "m_times":
            case "times":
            {
                if (value is not List<float> castValue) return false;
                instance.m_times = castValue;
                return true;
            }
            case "m_enum":
            case "enum":
            {
                if (value is null)
                {
                    instance.m_enum = default;
                    return true;
                }
                if (value is Havok.hk2019.hkxEnum castValue)
                {
                    instance.m_enum = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
