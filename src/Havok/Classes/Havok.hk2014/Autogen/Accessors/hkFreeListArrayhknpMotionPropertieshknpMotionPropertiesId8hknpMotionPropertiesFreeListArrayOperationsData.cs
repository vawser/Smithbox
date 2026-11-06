// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkFreeListArrayhknpMotionPropertieshknpMotionPropertiesId8hknpMotionPropertiesFreeListArrayOperationsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkFreeListArrayhknpMotionPropertieshknpMotionPropertiesId8hknpMotionPropertiesFreeListArrayOperations>
{
    public hkFreeListArrayhknpMotionPropertieshknpMotionPropertiesId8hknpMotionPropertiesFreeListArrayOperationsData(Havok.hk2014.hkFreeListArrayhknpMotionPropertieshknpMotionPropertiesId8hknpMotionPropertiesFreeListArrayOperations instance) : base(instance) { }

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
                if (value is not List<Havok.hk2014.hknpMotionProperties> castValue) return false;
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
