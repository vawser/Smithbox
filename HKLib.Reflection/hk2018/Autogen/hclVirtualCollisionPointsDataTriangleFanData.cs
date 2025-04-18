// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVirtualCollisionPointsDataTriangleFanData : HavokData<hclVirtualCollisionPointsData.TriangleFan> 
{
    public hclVirtualCollisionPointsDataTriangleFanData(HavokType type, hclVirtualCollisionPointsData.TriangleFan instance) : base(type, instance) {}

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
            case "m_vcpStartIndex":
            case "vcpStartIndex":
            {
                if (instance.m_vcpStartIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numTriangles":
            case "numTriangles":
            {
                if (instance.m_numTriangles is not TGet castValue) return false;
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
            case "m_vcpStartIndex":
            case "vcpStartIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_vcpStartIndex = castValue;
                return true;
            }
            case "m_numTriangles":
            case "numTriangles":
            {
                if (value is not byte castValue) return false;
                instance.m_numTriangles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
