// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpBallSocketChainDataConstraintInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpBallSocketChainDataConstraintInfo>
{
    public hkpBallSocketChainDataConstraintInfoData(Havok.hk2014.hkpBallSocketChainDataConstraintInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pivotInA":
            case "pivotInA":
            {
                value = instance.m_pivotInA;
                return true;
            }
            case "m_pivotInB":
            case "pivotInB":
            {
                value = instance.m_pivotInB;
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
            case "m_pivotInA":
            case "pivotInA":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_pivotInA = castValue;
                return true;
            }
            case "m_pivotInB":
            case "pivotInB":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_pivotInB = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
