// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkcdStaticAabbTreeData : HavokData<HKLib.hk2019.hkcdStaticAabbTree> 
{
    public hkcdStaticAabbTreeData(HavokType type, HKLib.hk2019.hkcdStaticAabbTree instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_treePtr":
            case "treePtr":
            {
                if (instance.m_treePtr is null)
                {
                    return true;
                }
                if (instance.m_treePtr is TGet castValue)
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
            case "m_treePtr":
            case "treePtr":
            {
                if (value is null)
                {
                    instance.m_treePtr = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkcdStaticAabbTree.Impl castValue)
                {
                    instance.m_treePtr = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
