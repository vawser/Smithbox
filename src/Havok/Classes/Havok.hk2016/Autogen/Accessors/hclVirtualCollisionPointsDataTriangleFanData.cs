// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclVirtualCollisionPointsDataTriangleFanData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclVirtualCollisionPointsData.TriangleFan>
{
    public hclVirtualCollisionPointsDataTriangleFanData(Havok.hk2016.hclVirtualCollisionPointsData.TriangleFan instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_realParticleIndex":
            case "realParticleIndex":
            {
                value = instance.m_realParticleIndex;
                return true;
            }
            case "m_vcpStartIndex":
            case "vcpStartIndex":
            {
                value = instance.m_vcpStartIndex;
                return true;
            }
            case "m_numTriangles":
            case "numTriangles":
            {
                value = instance.m_numTriangles;
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
