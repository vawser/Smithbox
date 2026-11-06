// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkxSparselyAnimatedBoolData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkxSparselyAnimatedBool>
{
    public hkxSparselyAnimatedBoolData(Havok.hk2014.hkxSparselyAnimatedBool instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bools":
            case "bools":
            {
                value = instance.m_bools;
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
