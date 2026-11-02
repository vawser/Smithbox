// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxSparselyAnimatedBoolData : HavokData<HKLib.hk2019.hkxSparselyAnimatedBool> 
{
    public hkxSparselyAnimatedBoolData(HavokType type, HKLib.hk2019.hkxSparselyAnimatedBool instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bools":
            case "bools":
            {
                if (instance.m_bools is not TGet castValue) return false;
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
            case "m_bools":
            case "bools":
            {
                if (value is not List<bool> castValue) return false;
                instance.m_bools = castValue;
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
