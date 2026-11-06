// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkcdDynamicTreeDefaultDynamicStoragehkcdDynamicTreeCodec32Data : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkcdDynamicTreeDefaultDynamicStoragehkcdDynamicTreeCodec32>
{
    public hkcdDynamicTreeDefaultDynamicStoragehkcdDynamicTreeCodec32Data(Havok.hk2014.hkcdDynamicTreeDefaultDynamicStoragehkcdDynamicTreeCodec32 instance) : base(instance) { }

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
                if (value is not List<Havok.hk2014.hkcdDynamicTree.Codec32> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_firstFree":
            case "firstFree":
            {
                if (value is not ushort castValue) return false;
                instance.m_firstFree = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
