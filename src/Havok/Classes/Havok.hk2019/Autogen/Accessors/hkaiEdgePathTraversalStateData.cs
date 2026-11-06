// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiEdgePathTraversalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiEdgePath.TraversalState>
{
    public hkaiEdgePathTraversalStateData(Havok.hk2019.hkaiEdgePath.TraversalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceEdge":
            case "faceEdge":
            {
                value = instance.m_faceEdge;
                return true;
            }
            case "m_trailingEdge":
            case "trailingEdge":
            {
                value = instance.m_trailingEdge;
                return true;
            }
            case "m_highestUserEdgeCrossed":
            case "highestUserEdgeCrossed":
            {
                value = instance.m_highestUserEdgeCrossed;
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
            case "m_faceEdge":
            case "faceEdge":
            {
                if (value is not int castValue) return false;
                instance.m_faceEdge = castValue;
                return true;
            }
            case "m_trailingEdge":
            case "trailingEdge":
            {
                if (value is not int castValue) return false;
                instance.m_trailingEdge = castValue;
                return true;
            }
            case "m_highestUserEdgeCrossed":
            case "highestUserEdgeCrossed":
            {
                if (value is not int castValue) return false;
                instance.m_highestUserEdgeCrossed = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
