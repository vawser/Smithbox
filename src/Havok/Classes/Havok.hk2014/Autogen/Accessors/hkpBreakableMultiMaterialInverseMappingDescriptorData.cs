// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpBreakableMultiMaterialInverseMappingDescriptorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpBreakableMultiMaterialInverseMappingDescriptor>
{
    public hkpBreakableMultiMaterialInverseMappingDescriptorData(Havok.hk2014.hkpBreakableMultiMaterialInverseMappingDescriptor instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_offset":
            case "offset":
            {
                value = instance.m_offset;
                return true;
            }
            case "m_numKeys":
            case "numKeys":
            {
                value = instance.m_numKeys;
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
            case "m_offset":
            case "offset":
            {
                if (value is not ushort castValue) return false;
                instance.m_offset = castValue;
                return true;
            }
            case "m_numKeys":
            case "numKeys":
            {
                if (value is not ushort castValue) return false;
                instance.m_numKeys = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
