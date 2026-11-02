// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkFreeListArrayHknpShapeInstanceData : HavokData<HKLib.hk2019.hkFreeListArrayHknpShapeInstance> 
{
    public hkFreeListArrayHknpShapeInstanceData(HavokType type, HKLib.hk2019.hkFreeListArrayHknpShapeInstance instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_elements":
            case "elements":
            {
                if (instance.m_elements is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_firstFree":
            case "firstFree":
            {
                if (instance.m_firstFree is not TGet castValue) return false;
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
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_elements":
            case "elements":
            {
                if (value is not List<HKLib.hk2019.hkFreeListArrayElementHknpShapeInstance> castValue) return false;
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
            case "m_numAllocated":
            case "numAllocated":
            {
                if (value is not uint castValue) return false;
                instance.m_numAllocated = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
