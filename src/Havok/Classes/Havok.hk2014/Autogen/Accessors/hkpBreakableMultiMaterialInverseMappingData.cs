// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpBreakableMultiMaterialInverseMappingData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpBreakableMultiMaterialInverseMapping>
{
    public hkpBreakableMultiMaterialInverseMappingData(Havok.hk2014.hkpBreakableMultiMaterialInverseMapping instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_descriptors":
            case "descriptors":
            {
                value = instance.m_descriptors;
                return true;
            }
            case "m_subShapeIds":
            case "subShapeIds":
            {
                value = instance.m_subShapeIds;
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
            case "m_descriptors":
            case "descriptors":
            {
                if (value is not List<Havok.hk2014.hkpBreakableMultiMaterialInverseMappingDescriptor> castValue) return false;
                instance.m_descriptors = castValue;
                return true;
            }
            case "m_subShapeIds":
            case "subShapeIds":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_subShapeIds = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
