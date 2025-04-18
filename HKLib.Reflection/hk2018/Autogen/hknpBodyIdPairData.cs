// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpBodyIdPairData : HavokData<hknpBodyIdPair> 
{
    public hknpBodyIdPairData(HavokType type, hknpBodyIdPair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bodyB":
            case "bodyB":
            {
                if (instance.m_bodyB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bodyA":
            case "bodyA":
            {
                if (instance.m_bodyA is not TGet castValue) return false;
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
            case "m_bodyB":
            case "bodyB":
            {
                if (value is not hknpBodyId castValue) return false;
                instance.m_bodyB = castValue;
                return true;
            }
            case "m_bodyA":
            case "bodyA":
            {
                if (value is not hknpBodyId castValue) return false;
                instance.m_bodyA = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
