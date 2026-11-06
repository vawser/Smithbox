// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkcdStaticTreeDynamicStoragehkcdStaticTreeCodec3Axis5Data : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkcdStaticTreeDynamicStoragehkcdStaticTreeCodec3Axis5>
{
    public hkcdStaticTreeDynamicStoragehkcdStaticTreeCodec3Axis5Data(Havok.hk2014.hkcdStaticTreeDynamicStoragehkcdStaticTreeCodec3Axis5 instance) : base(instance) { }

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
                if (value is not List<Havok.hk2014.hkcdStaticTreeCodec3Axis5> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
