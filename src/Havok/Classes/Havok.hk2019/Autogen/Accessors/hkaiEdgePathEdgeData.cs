// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiEdgePathEdgeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiEdgePath.Edge>
{
    public hkaiEdgePathEdgeData(Havok.hk2019.hkaiEdgePath.Edge instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_left":
            case "left":
            {
                value = instance.m_left;
                return true;
            }
            case "m_right":
            case "right":
            {
                value = instance.m_right;
                return true;
            }
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_followingTransform":
            case "followingTransform":
            {
                value = instance.m_followingTransform;
                return true;
            }
            case "m_edge":
            case "edge":
            {
                value = instance.m_edge;
                return true;
            }
            case "m_leftFollowingCorner":
            case "leftFollowingCorner":
            {
                value = instance.m_leftFollowingCorner;
                return true;
            }
            case "m_rightFollowingCorner":
            case "rightFollowingCorner":
            {
                value = instance.m_rightFollowingCorner;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
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
                if (value is not Havok.hk2019.hkaiPersistentEdgeKey castValue) return false;
                instance.m_edge = castValue;
                return true;
            }
            case "m_leftFollowingCorner":
            case "leftFollowingCorner":
            {
                if (value is not Havok.hk2019.hkaiEdgePath.FollowingCornerInfo castValue) return false;
                instance.m_leftFollowingCorner = castValue;
                return true;
            }
            case "m_rightFollowingCorner":
            case "rightFollowingCorner":
            {
                if (value is not Havok.hk2019.hkaiEdgePath.FollowingCornerInfo castValue) return false;
                instance.m_rightFollowingCorner = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2019.hkaiNavMesh.EdgeFlagBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (Havok.hk2019.hkaiNavMesh.EdgeFlagBits)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
