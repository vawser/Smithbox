// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshStaticTreeFaceIteratorData : HavokData<HKLib.hk2019.hkaiNavMeshStaticTreeFaceIterator> 
{
    public hkaiNavMeshStaticTreeFaceIteratorData(HavokType type, HKLib.hk2019.hkaiNavMeshStaticTreeFaceIterator instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_tree":
            case "tree":
            {
                if (instance.m_tree is null)
                {
                    return true;
                }
                if (instance.m_tree is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_tree":
            case "tree":
            {
                if (value is null)
                {
                    instance.m_tree = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkcdStaticAabbTree castValue)
                {
                    instance.m_tree = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
