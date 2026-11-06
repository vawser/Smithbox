// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkcdDynamicTreeCodecInt16Data : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkcdDynamicTree.CodecInt16>
{
    private static readonly System.Reflection.FieldInfo _childrenInfo = typeof(Havok.hk2014.hkcdDynamicTree.CodecInt16).GetField("m_children")!;
    public hkcdDynamicTreeCodecInt16Data(Havok.hk2014.hkcdDynamicTree.CodecInt16 instance) : base(instance) { }

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
            case "m_pad":
            case "pad":
            {
                value = instance.m_pad;
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
                if (value is not Havok.hk2014.hkcdDynamicTree.CodecInt16.IntAabb castValue) return false;
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
            case "m_pad":
            case "pad":
            {
                if (value is not uint castValue) return false;
                instance.m_pad = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
