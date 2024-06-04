// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMeshTextureRawBufferDescriptorData : HavokData<hkMeshTexture.RawBufferDescriptor> 
{
    public hkMeshTextureRawBufferDescriptorData(HavokType type, hkMeshTexture.RawBufferDescriptor instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_offset":
            case "offset":
            {
                if (instance.m_offset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stride":
            case "stride":
            {
                if (instance.m_stride is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numElements":
            case "numElements":
            {
                if (instance.m_numElements is not TGet castValue) return false;
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
