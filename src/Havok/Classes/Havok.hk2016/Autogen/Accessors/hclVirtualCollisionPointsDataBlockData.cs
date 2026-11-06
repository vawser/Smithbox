// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclVirtualCollisionPointsDataBlockData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclVirtualCollisionPointsData.Block>
{
    public hclVirtualCollisionPointsDataBlockData(Havok.hk2016.hclVirtualCollisionPointsData.Block instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_safeDisplacementRadius":
            case "safeDisplacementRadius":
            {
                value = instance.m_safeDisplacementRadius;
                return true;
            }
            case "m_startingVCPIndex":
            case "startingVCPIndex":
            {
                value = instance.m_startingVCPIndex;
                return true;
            }
            case "m_numVCPs":
            case "numVCPs":
            {
                value = instance.m_numVCPs;
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
            case "m_safeDisplacementRadius":
            case "safeDisplacementRadius":
            {
                if (value is not float castValue) return false;
                instance.m_safeDisplacementRadius = castValue;
                return true;
            }
            case "m_startingVCPIndex":
            case "startingVCPIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_startingVCPIndex = castValue;
                return true;
            }
            case "m_numVCPs":
            case "numVCPs":
            {
                if (value is not byte castValue) return false;
                instance.m_numVCPs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
