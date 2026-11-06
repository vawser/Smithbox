// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkxSparselyAnimatedBoolData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkxSparselyAnimatedBool>
{
    public hkxSparselyAnimatedBoolData(Havok.hk2018.hkxSparselyAnimatedBool instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
