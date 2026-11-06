// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkcdStaticTreeTreehkcdStaticTreeDynamicStorage32Data : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkcdStaticTreeTreehkcdStaticTreeDynamicStorage32>
{
    public hkcdStaticTreeTreehkcdStaticTreeDynamicStorage32Data(Havok.hk2014.hkcdStaticTreeTreehkcdStaticTreeDynamicStorage32 instance) : base(instance) { }

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
                if (value is not List<Havok.hk2014.hkcdStaticTreeCodecRaw> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                if (value is not Havok.hk2014.hkAabb castValue) return false;
                instance.m_domain = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
