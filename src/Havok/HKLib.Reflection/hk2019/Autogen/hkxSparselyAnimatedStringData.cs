// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxSparselyAnimatedStringData : HavokData<HKLib.hk2019.hkxSparselyAnimatedString> 
{
    public hkxSparselyAnimatedStringData(HavokType type, HKLib.hk2019.hkxSparselyAnimatedString instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_strings":
            case "strings":
            {
                if (instance.m_strings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_times":
            case "times":
            {
                if (instance.m_times is not TGet castValue) return false;
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
