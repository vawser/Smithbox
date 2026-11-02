// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpThreadSafeObjectPoolHknpMaterialData : HavokData<HKLib.hk2019.hknpThreadSafeObjectPoolHknpMaterial> 
{
    public hknpThreadSafeObjectPoolHknpMaterialData(HavokType type, HKLib.hk2019.hknpThreadSafeObjectPoolHknpMaterial instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_objects":
            case "objects":
            {
                if (instance.m_objects is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_freeListHead":
            case "freeListHead":
            {
                if (instance.m_freeListHead is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numAllocated":
            case "numAllocated":
            {
                if (instance.m_numAllocated is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_peakIndex":
            case "peakIndex":
            {
                if (instance.m_peakIndex is not TGet castValue) return false;
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
            case "m_objects":
            case "objects":
            {
                if (value is not List<HKLib.hk2019.hknpThreadSafeObjectPoolElementHknpMaterial> castValue) return false;
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
