// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkBitFieldBaseData<Storage> : HavokData<hkBitFieldBase<Storage>> 
{
    public hkBitFieldBaseData(HavokType type, hkBitFieldBase<Storage> instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_storage":
            case "storage":
            {
                if (instance.m_storage is null)
                {
                    return true;
                }
                if (instance.m_storage is TGet castValue)
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
            case "m_storage":
            case "storage":
            {
                if (value is null)
                {
                    instance.m_storage = default;
                    return true;
                }
                if (value is Storage castValue)
                {
                    instance.m_storage = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
