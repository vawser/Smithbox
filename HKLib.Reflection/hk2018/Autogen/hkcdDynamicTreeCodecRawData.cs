// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkcdDynamicTree;

namespace HKLib.Reflection.hk2018;

internal class hkcdDynamicTreeCodecRawData<INDEX_TYPE> : HavokData<CodecRaw<INDEX_TYPE>> 
{
    private static readonly System.Reflection.FieldInfo _childrenInfo = typeof(CodecRaw<INDEX_TYPE>).GetField("m_children")!;
    public hkcdDynamicTreeCodecRawData(HavokType type, CodecRaw<INDEX_TYPE> instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aabb":
            case "aabb":
            {
                if (instance.m_aabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_parent":
            case "parent":
            {
                if (instance.m_parent is null)
                {
                    return true;
                }
                if (instance.m_parent is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_children":
            case "children":
            {
                if (instance.m_children is null)
                {
                    return true;
                }
                if (instance.m_children is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_aabb":
            case "aabb":
            {
                if (value is not hkAabb castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            case "m_parent":
            case "parent":
            {
                if (value is null)
                {
                    instance.m_parent = default;
                    return true;
                }
                if (value is INDEX_TYPE castValue)
                {
                    instance.m_parent = castValue;
                    return true;
                }
                return false;
            }
            case "m_children":
            case "children":
            {
                if (value is not INDEX_TYPE[] castValue || castValue.Length != 2) return false;
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
