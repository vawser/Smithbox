// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hk.RPC;

namespace HKLib.Reflection.hk2018;

internal class hkRPCOnAfterSerializeData : HavokData<OnAfterSerialize> 
{
    public hkRPCOnAfterSerializeData(HavokType type, OnAfterSerialize instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_onAfterSerialize":
            case "onAfterSerialize":
            {
                if (instance.m_onAfterSerialize is null)
                {
                    return true;
                }
                if (instance.m_onAfterSerialize is TGet castValue)
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
            case "m_onAfterSerialize":
            case "onAfterSerialize":
            {
                if (value is null)
                {
                    instance.m_onAfterSerialize = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_onAfterSerialize = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
