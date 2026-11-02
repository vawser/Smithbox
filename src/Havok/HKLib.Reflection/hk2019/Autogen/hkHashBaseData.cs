// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkHashBaseData<ITEM> : HavokData<HKLib.hk2019.hkHashBase<ITEM>> 
{
    public hkHashBaseData(HavokType type, HKLib.hk2019.hkHashBase<ITEM> instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_items":
            case "items":
            {
                if (instance.m_items is not TGet castValue) return false;
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
            case "m_items":
            case "items":
            {
                if (value is not List<ITEM> castValue) return false;
                instance.m_items = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
