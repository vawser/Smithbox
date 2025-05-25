// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkcdDynamicTree;

namespace HKLib.Reflection.hk2018;

internal class hkcdDynamicTreeCodecRawUlongData : HavokData<CodecRawUlong> 
{
    private static readonly System.Reflection.FieldInfo _childrenInfo = typeof(CodecRawUlong).GetField("m_children")!;
    public hkcdDynamicTreeCodecRawUlongData(HavokType type, CodecRawUlong instance) : base(type, instance) {}

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
                if (instance.m_parent is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_children":
            case "children":
            {
                if (instance.m_children is not TGet castValue) return false;
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
                if (value is not ulong castValue) return false;
                instance.m_parent = castValue;
                return true;
            }
            case "m_children":
            case "children":
            {
                if (value is not ulong[] castValue || castValue.Length != 2) return false;
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
