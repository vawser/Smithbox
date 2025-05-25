// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkcdDynamicTree;

namespace HKLib.Reflection.hk2018;

internal class hkcdDynamicTreeTreeDefaultDynamicStorageCodec32Data : HavokData<TreeDefaultDynamicStorageCodec32> 
{
    public hkcdDynamicTreeTreeDefaultDynamicStorageCodec32Data(HavokType type, TreeDefaultDynamicStorageCodec32 instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodes":
            case "nodes":
            {
                if (instance.m_nodes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_firstFree":
            case "firstFree":
            {
                if (instance.m_firstFree is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numLeaves":
            case "numLeaves":
            {
                if (instance.m_numLeaves is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_path":
            case "path":
            {
                if (instance.m_path is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_root":
            case "root":
            {
                if (instance.m_root is not TGet castValue) return false;
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
            case "m_nodes":
            case "nodes":
            {
                if (value is not List<Codec32> castValue) return false;
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
            case "m_numLeaves":
            case "numLeaves":
            {
                if (value is not uint castValue) return false;
                instance.m_numLeaves = castValue;
                return true;
            }
            case "m_path":
            case "path":
            {
                if (value is not uint castValue) return false;
                instance.m_path = castValue;
                return true;
            }
            case "m_root":
            case "root":
            {
                if (value is not ushort castValue) return false;
                instance.m_root = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
