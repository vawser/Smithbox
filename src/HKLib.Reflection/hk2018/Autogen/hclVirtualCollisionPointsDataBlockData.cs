// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVirtualCollisionPointsDataBlockData : HavokData<hclVirtualCollisionPointsData.Block> 
{
    public hclVirtualCollisionPointsDataBlockData(HavokType type, hclVirtualCollisionPointsData.Block instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_safeDisplacementRadius":
            case "safeDisplacementRadius":
            {
                if (instance.m_safeDisplacementRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startingVCPIndex":
            case "startingVCPIndex":
            {
                if (instance.m_startingVCPIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numVCPs":
            case "numVCPs":
            {
                if (instance.m_numVCPs is not TGet castValue) return false;
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
