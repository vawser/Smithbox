// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkAssetRefPtrData<TYPE> : HavokData<hkAssetRefPtr<TYPE>> 
{
    public hkAssetRefPtrData(HavokType type, hkAssetRefPtr<TYPE> instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_ptr":
            case "ptr":
            {
                if (instance.m_ptr is null)
                {
                    return true;
                }
                if (instance.m_ptr is TGet castValue)
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
            case "m_ptr":
            case "ptr":
            {
                if (value is null)
                {
                    instance.m_ptr = default;
                    return true;
                }
                if (value is TYPE castValue)
                {
                    instance.m_ptr = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
