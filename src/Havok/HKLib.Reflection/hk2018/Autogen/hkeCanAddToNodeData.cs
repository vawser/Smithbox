// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hke;

namespace HKLib.Reflection.hk2018;

internal class hkeCanAddToNodeData : HavokData<CanAddToNode> 
{
    public hkeCanAddToNodeData(HavokType type, CanAddToNode instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_canAddToNodeFunc":
            case "canAddToNodeFunc":
            {
                if (instance.m_canAddToNodeFunc is null)
                {
                    return true;
                }
                if (instance.m_canAddToNodeFunc is TGet castValue)
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
            case "m_canAddToNodeFunc":
            case "canAddToNodeFunc":
            {
                if (value is null)
                {
                    instance.m_canAddToNodeFunc = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_canAddToNodeFunc = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
