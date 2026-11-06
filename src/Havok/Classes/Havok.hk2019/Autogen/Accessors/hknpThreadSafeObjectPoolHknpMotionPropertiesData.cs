// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpThreadSafeObjectPoolHknpMotionPropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpThreadSafeObjectPoolHknpMotionProperties>
{
    public hknpThreadSafeObjectPoolHknpMotionPropertiesData(Havok.hk2019.hknpThreadSafeObjectPoolHknpMotionProperties instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_objects":
            case "objects":
            {
                value = instance.m_objects;
                return true;
            }
            case "m_freeListHead":
            case "freeListHead":
            {
                value = instance.m_freeListHead;
                return true;
            }
            case "m_numAllocated":
            case "numAllocated":
            {
                value = instance.m_numAllocated;
                return true;
            }
            case "m_peakIndex":
            case "peakIndex":
            {
                value = instance.m_peakIndex;
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
            case "m_objects":
            case "objects":
            {
                if (value is not List<Havok.hk2019.hknpThreadSafeObjectPoolElementHknpMotionProperties> castValue) return false;
                instance.m_objects = castValue;
                return true;
            }
            case "m_freeListHead":
            case "freeListHead":
            {
                if (value is not uint castValue) return false;
                instance.m_freeListHead = castValue;
                return true;
            }
            case "m_numAllocated":
            case "numAllocated":
            {
                if (value is not uint castValue) return false;
                instance.m_numAllocated = castValue;
                return true;
            }
            case "m_peakIndex":
            case "peakIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_peakIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
