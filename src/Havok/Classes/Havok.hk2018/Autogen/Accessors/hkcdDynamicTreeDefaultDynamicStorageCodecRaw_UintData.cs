// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkcdDynamicTreeDefaultDynamicStorageCodecRaw_UintData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkcdDynamicTree.DefaultDynamicStorageCodecRaw_Uint>
{
    public hkcdDynamicTreeDefaultDynamicStorageCodecRaw_UintData(Havok.hk2018.hkcdDynamicTree.DefaultDynamicStorageCodecRaw_Uint instance) : base(instance) { }

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
                if (value is not List<Havok.hk2018.hkcdDynamicTree.CodecRaw<uint>> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_firstFree":
            case "firstFree":
            {
                if (value is not uint castValue) return false;
                instance.m_firstFree = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
