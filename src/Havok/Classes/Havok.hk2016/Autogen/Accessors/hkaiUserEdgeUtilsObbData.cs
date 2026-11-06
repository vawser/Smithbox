// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkaiUserEdgeUtilsObbData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkaiUserEdgeUtils.Obb>
{
    public hkaiUserEdgeUtilsObbData(Havok.hk2016.hkaiUserEdgeUtils.Obb instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
                return true;
            }
            case "m_halfExtents":
            case "halfExtents":
            {
                value = instance.m_halfExtents;
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
            case "m_transform":
            case "transform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_halfExtents":
            case "halfExtents":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_halfExtents = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
