// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshEdgeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMesh.Edge>
{
    public hkaiNavMeshEdgeData(Havok.hk2018.hkaiNavMesh.Edge instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_a":
            case "a":
            {
                value = instance.m_a;
                return true;
            }
            case "m_b":
            case "b":
            {
                value = instance.m_b;
                return true;
            }
            case "m_oppositeEdge":
            case "oppositeEdge":
            {
                value = instance.m_oppositeEdge;
                return true;
            }
            case "m_oppositeFace":
            case "oppositeFace":
            {
                value = instance.m_oppositeFace;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_userEdgeCost":
            case "userEdgeCost":
            {
                value = instance.m_userEdgeCost;
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
            case "m_a":
            case "a":
            {
                if (value is not int castValue) return false;
                instance.m_a = castValue;
                return true;
            }
            case "m_b":
            case "b":
            {
                if (value is not int castValue) return false;
                instance.m_b = castValue;
                return true;
            }
            case "m_oppositeEdge":
            case "oppositeEdge":
            {
                if (value is not uint castValue) return false;
                instance.m_oppositeEdge = castValue;
                return true;
            }
            case "m_oppositeFace":
            case "oppositeFace":
            {
                if (value is not uint castValue) return false;
                instance.m_oppositeFace = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2018.hkaiNavMesh.EdgeFlagBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (Havok.hk2018.hkaiNavMesh.EdgeFlagBits)byteValue;
                    return true;
                }
                return false;
            }
            case "m_userEdgeCost":
            case "userEdgeCost":
            {
                if (value is not float castValue) return false;
                instance.m_userEdgeCost = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
