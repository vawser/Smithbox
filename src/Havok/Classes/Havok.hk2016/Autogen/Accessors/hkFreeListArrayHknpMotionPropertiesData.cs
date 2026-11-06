// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkFreeListArrayHknpMotionPropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkFreeListArrayHknpMotionProperties>
{
    public hkFreeListArrayHknpMotionPropertiesData(Havok.hk2016.hkFreeListArrayHknpMotionProperties instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_elements":
            case "elements":
            {
                value = instance.m_elements;
                return true;
            }
            case "m_firstFree":
            case "firstFree":
            {
                value = instance.m_firstFree;
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
            case "m_elements":
            case "elements":
            {
                if (value is not List<Havok.hk2016.hkFreeListArrayElementHknpMotionProperties> castValue) return false;
                instance.m_elements = castValue;
                return true;
            }
            case "m_firstFree":
            case "firstFree":
            {
                if (value is not int castValue) return false;
                instance.m_firstFree = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
