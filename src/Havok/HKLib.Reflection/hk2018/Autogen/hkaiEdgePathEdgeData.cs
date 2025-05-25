// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiEdgePathEdgeData : HavokData<hkaiEdgePath.Edge> 
{
    public hkaiEdgePathEdgeData(HavokType type, hkaiEdgePath.Edge instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_left":
            case "left":
            {
                if (instance.m_left is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_right":
            case "right":
            {
                if (instance.m_right is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_followingTransform":
            case "followingTransform":
            {
                if (instance.m_followingTransform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edge":
            case "edge":
            {
                if (instance.m_edge is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_leftFollowingCorner":
            case "leftFollowingCorner":
            {
                if (instance.m_leftFollowingCorner is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rightFollowingCorner":
            case "rightFollowingCorner":
            {
                if (instance.m_rightFollowingCorner is not TGet castValue) return false;
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
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_left":
            case "left":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_left = castValue;
                return true;
            }
            case "m_right":
            case "right":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_right = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_followingTransform":
            case "followingTransform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_followingTransform = castValue;
                return true;
            }
            case "m_edge":
            case "edge":
            {
                if (value is not hkaiPersistentEdgeKey castValue) return false;
                instance.m_edge = castValue;
                return true;
            }
            case "m_leftFollowingCorner":
            case "leftFollowingCorner":
            {
                if (value is not hkaiEdgePath.FollowingCornerInfo castValue) return false;
                instance.m_leftFollowingCorner = castValue;
                return true;
            }
            case "m_rightFollowingCorner":
            case "rightFollowingCorner":
            {
                if (value is not hkaiEdgePath.FollowingCornerInfo castValue) return false;
                instance.m_rightFollowingCorner = castValue;
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
            default:
            return false;
        }
    }

}
