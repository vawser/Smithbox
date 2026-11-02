// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkOffsetBitFieldData : HavokData<HKLib.hk2019.hkOffsetBitField> 
{
    public hkOffsetBitFieldData(HavokType type, HKLib.hk2019.hkOffsetBitField instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_storage":
            case "storage":
            {
                if (instance.m_storage is not TGet castValue) return false;
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
            case "m_storage":
            case "storage":
            {
                if (value is not HKLib.hk2019.hkOffsetBitFieldStorage<List<uint>> castValue) return false;
                instance.m_storage = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
