// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkcdStaticTreeAabb5BytesTreeData : HavokData<HKLib.hk2019.hkcdStaticTree.Aabb5BytesTree> 
{
    public hkcdStaticTreeAabb5BytesTreeData(HavokType type, HKLib.hk2019.hkcdStaticTree.Aabb5BytesTree instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodes":
            case "nodes":
            {
                if (instance.m_nodes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                if (instance.m_domain is not TGet castValue) return false;
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
            case "m_nodes":
            case "nodes":
            {
                if (value is not List<HKLib.hk2019.hkcdCompressedAabbCodecs.Aabb5BytesCodec> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                if (value is not HKLib.hk2019.hkAabb castValue) return false;
                instance.m_domain = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
