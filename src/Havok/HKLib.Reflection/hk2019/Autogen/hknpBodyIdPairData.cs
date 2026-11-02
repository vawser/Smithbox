// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpBodyIdPairData : HavokData<HKLib.hk2019.hknpBodyIdPair> 
{
    public hknpBodyIdPairData(HavokType type, HKLib.hk2019.hknpBodyIdPair instance) : base(type, instance) {}

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
                if (value is not HKLib.hk2019.hknpBodyId castValue) return false;
                instance.m_bodyB = castValue;
                return true;
            }
            case "m_bodyA":
            case "bodyA":
            {
                if (value is not HKLib.hk2019.hknpBodyId castValue) return false;
                instance.m_bodyA = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
