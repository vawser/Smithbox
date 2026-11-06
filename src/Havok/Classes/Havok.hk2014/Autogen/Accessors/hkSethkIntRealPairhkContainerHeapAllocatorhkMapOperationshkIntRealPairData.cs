// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkSethkIntRealPairhkContainerHeapAllocatorhkMapOperationshkIntRealPairData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkSethkIntRealPairhkContainerHeapAllocatorhkMapOperationshkIntRealPair>
{
    public hkSethkIntRealPairhkContainerHeapAllocatorhkMapOperationshkIntRealPairData(Havok.hk2014.hkSethkIntRealPairhkContainerHeapAllocatorhkMapOperationshkIntRealPair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_elem":
            case "elem":
            {
                value = instance.m_elem;
                return true;
            }
            case "m_numElems":
            case "numElems":
            {
                value = instance.m_numElems;
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
            case "m_elem":
            case "elem":
            {
                if (value is not List<Havok.hk2014.hkIntRealPair> castValue) return false;
                instance.m_elem = castValue;
                return true;
            }
            case "m_numElems":
            case "numElems":
            {
                if (value is not int castValue) return false;
                instance.m_numElems = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
