// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkcdSimdTreeNodeData : HavokData<hkcdSimdTree.Node> 
{
    private static readonly System.Reflection.FieldInfo _dataInfo = typeof(hkcdSimdTree.Node).GetField("m_data")!;
    public hkcdSimdTreeNodeData(HavokType type, hkcdSimdTree.Node instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_lx":
            case "lx":
            {
                if (instance.m_lx is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hx":
            case "hx":
            {
                if (instance.m_hx is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ly":
            case "ly":
            {
                if (instance.m_ly is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hy":
            case "hy":
            {
                if (instance.m_hy is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lz":
            case "lz":
            {
                if (instance.m_lz is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hz":
            case "hz":
            {
                if (instance.m_hz is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (instance.m_data is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isLeaf":
            case "isLeaf":
            {
                if (instance.m_isLeaf is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
            case "m_isLeaf":
            case "isLeaf":
            {
                if (value is not bool castValue) return false;
                instance.m_isLeaf = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
