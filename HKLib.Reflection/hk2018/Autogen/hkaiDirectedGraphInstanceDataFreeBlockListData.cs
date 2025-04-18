// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2018;

internal class hkaiDirectedGraphInstanceDataFreeBlockListData : HavokData<HKLib.hk2018.hkaiDirectedGraphInstanceData.FreeBlockList> 
{
    public hkaiDirectedGraphInstanceDataFreeBlockListData(HavokType type, HKLib.hk2018.hkaiDirectedGraphInstanceData.FreeBlockList instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_blocks":
            case "blocks":
            {
                if (instance.m_blocks is not TGet castValue) return false;
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
            case "m_blocks":
            case "blocks":
            {
                if (value is not List<int> castValue) return false;
                instance.m_blocks = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
