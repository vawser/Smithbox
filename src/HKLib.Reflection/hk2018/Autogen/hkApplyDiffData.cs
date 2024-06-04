// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hk;

namespace HKLib.Reflection.hk2018;

internal class hkApplyDiffData : HavokData<ApplyDiff> 
{
    public hkApplyDiffData(HavokType type, ApplyDiff instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_fn":
            case "fn":
            {
                if (instance.m_fn is null)
                {
                    return true;
                }
                if (instance.m_fn is TGet castValue)
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
            case "m_fn":
            case "fn":
            {
                if (value is null)
                {
                    instance.m_fn = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_fn = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
