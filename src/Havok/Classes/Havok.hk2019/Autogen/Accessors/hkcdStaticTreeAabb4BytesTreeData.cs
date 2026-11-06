// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdStaticTreeAabb4BytesTreeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdStaticTree.Aabb4BytesTree>
{
    public hkcdStaticTreeAabb4BytesTreeData(Havok.hk2019.hkcdStaticTree.Aabb4BytesTree instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodes":
            case "nodes":
            {
                value = instance.m_nodes;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                value = instance.m_domain;
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
            case "m_nodes":
            case "nodes":
            {
                if (value is not List<Havok.hk2019.hkcdCompressedAabbCodecs.Aabb4BytesCodec> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                if (value is not Havok.hk2019.hkAabb castValue) return false;
                instance.m_domain = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
