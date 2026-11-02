// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkcdDynamicAabbTreeImplData : HavokData<HKLib.hk2019.hkcdDynamicAabbTree.Impl> 
{
    public hkcdDynamicAabbTreeImplData(HavokType type, HKLib.hk2019.hkcdDynamicAabbTree.Impl instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_tree":
            case "tree":
            {
                if (instance.m_tree is not TGet castValue) return false;
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
            case "m_tree":
            case "tree":
            {
                if (value is not HKLib.hk2019.hkcdDynamicTree.DefaultTree48 castValue) return false;
                instance.m_tree = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
