// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkcdSimdTreeNodeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkcdSimdTree.Node>
{
    private static readonly System.Reflection.FieldInfo _dataInfo = typeof(Havok.hk2018.hkcdSimdTree.Node).GetField("m_data")!;
    public hkcdSimdTreeNodeData(Havok.hk2018.hkcdSimdTree.Node instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_lx":
            case "lx":
            {
                value = instance.m_lx;
                return true;
            }
            case "m_hx":
            case "hx":
            {
                value = instance.m_hx;
                return true;
            }
            case "m_ly":
            case "ly":
            {
                value = instance.m_ly;
                return true;
            }
            case "m_hy":
            case "hy":
            {
                value = instance.m_hy;
                return true;
            }
            case "m_lz":
            case "lz":
            {
                value = instance.m_lz;
                return true;
            }
            case "m_hz":
            case "hz":
            {
                value = instance.m_hz;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            case "m_parent":
            case "parent":
            {
                value = instance.m_parent;
                return true;
            }
            case "m_isLeaf":
            case "isLeaf":
            {
                value = instance.m_isLeaf;
                return true;
            }
            case "m_isActive":
            case "isActive":
            {
                value = instance.m_isActive;
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
            case "m_lx":
            case "lx":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_lx = castValue;
                return true;
            }
            case "m_hx":
            case "hx":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_hx = castValue;
                return true;
            }
            case "m_ly":
            case "ly":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_ly = castValue;
                return true;
            }
            case "m_hy":
            case "hy":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_hy = castValue;
                return true;
            }
            case "m_lz":
            case "lz":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_lz = castValue;
                return true;
            }
            case "m_hz":
            case "hz":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_hz = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (value is not uint[] castValue || castValue.Length != 4) return false;
                try
                {
                    _dataInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_parent":
            case "parent":
            {
                if (value is not uint castValue) return false;
                instance.m_parent = castValue;
                return true;
            }
            case "m_isLeaf":
            case "isLeaf":
            {
                if (value is not bool castValue) return false;
                instance.m_isLeaf = castValue;
                return true;
            }
            case "m_isActive":
            case "isActive":
            {
                if (value is not bool castValue) return false;
                instance.m_isActive = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
