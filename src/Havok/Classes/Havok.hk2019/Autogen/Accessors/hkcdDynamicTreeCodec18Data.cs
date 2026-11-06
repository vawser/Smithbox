// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdDynamicTreeCodec18Data : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdDynamicTree.Codec18>
{
    public hkcdDynamicTreeCodec18Data(Havok.hk2019.hkcdDynamicTree.Codec18 instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aabb":
            case "aabb":
            {
                value = instance.m_aabb;
                return true;
            }
            case "m_parent":
            case "parent":
            {
                value = instance.m_parent;
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
            case "m_aabb":
            case "aabb":
            {
                if (value is not Havok.hk2019.hkAabbHalf castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            case "m_parent":
            case "parent":
            {
                if (value is not ushort castValue) return false;
                instance.m_parent = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
