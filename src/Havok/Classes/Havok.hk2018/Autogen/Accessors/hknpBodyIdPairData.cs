// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpBodyIdPairData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpBodyIdPair>
{
    public hknpBodyIdPairData(Havok.hk2018.hknpBodyIdPair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bodyB":
            case "bodyB":
            {
                value = instance.m_bodyB;
                return true;
            }
            case "m_bodyA":
            case "bodyA":
            {
                value = instance.m_bodyA;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_bodyB":
            case "bodyB":
            {
                if (value is not Havok.hk2018.hknpBodyId castValue) return false;
                instance.m_bodyB = castValue;
                return true;
            }
            case "m_bodyA":
            case "bodyA":
            {
                if (value is not Havok.hk2018.hknpBodyId castValue) return false;
                instance.m_bodyA = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
