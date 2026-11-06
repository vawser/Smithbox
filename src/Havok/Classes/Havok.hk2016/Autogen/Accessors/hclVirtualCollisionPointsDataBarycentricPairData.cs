// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclVirtualCollisionPointsDataBarycentricPairData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclVirtualCollisionPointsData.BarycentricPair>
{
    public hclVirtualCollisionPointsDataBarycentricPairData(Havok.hk2016.hclVirtualCollisionPointsData.BarycentricPair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_u":
            case "u":
            {
                value = instance.m_u;
                return true;
            }
            case "m_v":
            case "v":
            {
                value = instance.m_v;
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
            case "m_u":
            case "u":
            {
                if (value is not float castValue) return false;
                instance.m_u = castValue;
                return true;
            }
            case "m_v":
            case "v":
            {
                if (value is not float castValue) return false;
                instance.m_v = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
