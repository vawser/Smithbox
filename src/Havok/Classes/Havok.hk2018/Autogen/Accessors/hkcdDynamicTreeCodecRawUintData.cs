// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkcdDynamicTreeCodecRawUintData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkcdDynamicTree.CodecRawUint>
{
    private static readonly System.Reflection.FieldInfo _childrenInfo = typeof(Havok.hk2018.hkcdDynamicTree.CodecRawUint).GetField("m_children")!;
    public hkcdDynamicTreeCodecRawUintData(Havok.hk2018.hkcdDynamicTree.CodecRawUint instance) : base(instance) { }

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
            case "m_children":
            case "children":
            {
                value = instance.m_children;
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
                if (value is not Havok.hk2018.hkAabb castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            case "m_parent":
            case "parent":
            {
                if (value is not uint castValue) return false;
                instance.m_parent = castValue;
                return true;
            }
            case "m_children":
            case "children":
            {
                if (value is not uint[] castValue || castValue.Length != 2) return false;
                try
                {
                    _childrenInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
