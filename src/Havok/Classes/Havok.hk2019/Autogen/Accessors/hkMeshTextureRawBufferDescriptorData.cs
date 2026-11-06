// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkMeshTextureRawBufferDescriptorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkMeshTexture.RawBufferDescriptor>
{
    public hkMeshTextureRawBufferDescriptorData(Havok.hk2019.hkMeshTexture.RawBufferDescriptor instance) : base(instance) { }

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
            case "m_stride":
            case "stride":
            {
                value = instance.m_stride;
                return true;
            }
            case "m_numElements":
            case "numElements":
            {
                value = instance.m_numElements;
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
                if (value is not long castValue) return false;
                instance.m_offset = castValue;
                return true;
            }
            case "m_stride":
            case "stride":
            {
                if (value is not uint castValue) return false;
                instance.m_stride = castValue;
                return true;
            }
            case "m_numElements":
            case "numElements":
            {
                if (value is not uint castValue) return false;
                instance.m_numElements = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
