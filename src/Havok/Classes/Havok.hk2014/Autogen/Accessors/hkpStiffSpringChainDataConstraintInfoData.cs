// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpStiffSpringChainDataConstraintInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpStiffSpringChainDataConstraintInfo>
{
    public hkpStiffSpringChainDataConstraintInfoData(Havok.hk2014.hkpStiffSpringChainDataConstraintInfo instance) : base(instance) { }

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
            case "m_springLength":
            case "springLength":
            {
                value = instance.m_springLength;
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
            case "m_springLength":
            case "springLength":
            {
                if (value is not float castValue) return false;
                instance.m_springLength = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
