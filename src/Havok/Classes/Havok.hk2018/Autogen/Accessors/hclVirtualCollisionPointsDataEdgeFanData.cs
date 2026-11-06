// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclVirtualCollisionPointsDataEdgeFanData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclVirtualCollisionPointsData.EdgeFan>
{
    public hclVirtualCollisionPointsDataEdgeFanData(Havok.hk2018.hclVirtualCollisionPointsData.EdgeFan instance) : base(instance) { }

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
            case "m_edgeStartIndex":
            case "edgeStartIndex":
            {
                value = instance.m_edgeStartIndex;
                return true;
            }
            case "m_numEdges":
            case "numEdges":
            {
                value = instance.m_numEdges;
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
            case "m_edgeStartIndex":
            case "edgeStartIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_edgeStartIndex = castValue;
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
