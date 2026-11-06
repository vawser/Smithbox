// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiAffineMap1DData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiAffineMap1D>
{
    public hkaiAffineMap1DData(Havok.hk2019.hkaiAffineMap1D instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_scale":
            case "scale":
            {
                value = instance.m_scale;
                return true;
            }
            case "m_offset":
            case "offset":
            {
                value = instance.m_offset;
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
            case "m_scale":
            case "scale":
            {
                if (value is not float castValue) return false;
                instance.m_scale = castValue;
                return true;
            }
            case "m_offset":
            case "offset":
            {
                if (value is not float castValue) return false;
                instance.m_offset = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
