// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdDynamicTreeDefaultDynamicStorageCodecInt16Data : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdDynamicTree.DefaultDynamicStorageCodecInt16>
{
    public hkcdDynamicTreeDefaultDynamicStorageCodecInt16Data(Havok.hk2019.hkcdDynamicTree.DefaultDynamicStorageCodecInt16 instance) : base(instance) { }

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
                if (value is not List<Havok.hk2019.hkcdDynamicTree.CodecInt16> castValue) return false;
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
