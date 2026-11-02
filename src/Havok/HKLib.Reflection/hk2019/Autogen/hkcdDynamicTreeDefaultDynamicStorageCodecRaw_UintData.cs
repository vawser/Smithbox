// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkcdDynamicTreeDefaultDynamicStorageCodecRaw_UintData : HavokData<HKLib.hk2019.hkcdDynamicTree.DefaultDynamicStorageCodecRaw_Uint> 
{
    public hkcdDynamicTreeDefaultDynamicStorageCodecRaw_UintData(HavokType type, HKLib.hk2019.hkcdDynamicTree.DefaultDynamicStorageCodecRaw_Uint instance) : base(type, instance) {}

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
                if (value is not List<HKLib.hk2019.hkcdDynamicTree.CodecRaw<uint>> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_firstFree":
            case "firstFree":
            {
                if (value is not uint castValue) return false;
                instance.m_firstFree = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
