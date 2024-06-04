// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshEdgeData : HavokData<hkaiNavMesh.Edge> 
{
    public hkaiNavMeshEdgeData(HavokType type, hkaiNavMesh.Edge instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_a":
            case "a":
            {
                if (instance.m_a is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_b":
            case "b":
            {
                if (instance.m_b is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_oppositeEdge":
            case "oppositeEdge":
            {
                if (instance.m_oppositeEdge is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_oppositeFace":
            case "oppositeFace":
            {
                if (instance.m_oppositeFace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_flags is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_userEdgeCost":
            case "userEdgeCost":
            {
                if (instance.m_userEdgeCost is not TGet castValue) return false;
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
                if (value is hkaiNavMesh.EdgeFlagBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (hkaiNavMesh.EdgeFlagBits)byteValue;
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
