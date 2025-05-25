// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVirtualCollisionPointsDataEdgeFanLandscapeData : HavokData<hclVirtualCollisionPointsData.EdgeFanLandscape> 
{
    public hclVirtualCollisionPointsDataEdgeFanLandscapeData(HavokType type, hclVirtualCollisionPointsData.EdgeFanLandscape instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_realParticleIndex":
            case "realParticleIndex":
            {
                if (instance.m_realParticleIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeStartIndex":
            case "edgeStartIndex":
            {
                if (instance.m_edgeStartIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vcpStartIndex":
            case "vcpStartIndex":
            {
                if (instance.m_vcpStartIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numEdges":
            case "numEdges":
            {
                if (instance.m_numEdges is not TGet castValue) return false;
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
            case "m_realParticleIndex":
            case "realParticleIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_realParticleIndex = castValue;
                return true;
            }
            case "m_edgeStartIndex":
            case "edgeStartIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_edgeStartIndex = castValue;
                return true;
            }
            case "m_vcpStartIndex":
            case "vcpStartIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_vcpStartIndex = castValue;
                return true;
            }
            case "m_numEdges":
            case "numEdges":
            {
                if (value is not byte castValue) return false;
                instance.m_numEdges = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
