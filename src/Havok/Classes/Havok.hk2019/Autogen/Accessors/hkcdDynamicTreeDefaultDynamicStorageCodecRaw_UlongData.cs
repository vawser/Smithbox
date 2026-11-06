// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdDynamicTreeDefaultDynamicStorageCodecRaw_UlongData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdDynamicTree.DefaultDynamicStorageCodecRaw_Ulong>
{
    public hkcdDynamicTreeDefaultDynamicStorageCodecRaw_UlongData(Havok.hk2019.hkcdDynamicTree.DefaultDynamicStorageCodecRaw_Ulong instance) : base(instance) { }

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
            case "m_firstFree":
            case "firstFree":
            {
                value = instance.m_firstFree;
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
                if (value is not List<Havok.hk2019.hkcdDynamicTree.CodecRaw<ulong>> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_firstFree":
            case "firstFree":
            {
                if (value is not ulong castValue) return false;
                instance.m_firstFree = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
