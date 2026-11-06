// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpShapeInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpShapeInfo>
{
    public hkpShapeInfoData(Havok.hk2014.hkpShapeInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_shape":
            case "shape":
            {
                value = instance.m_shape;
                return true;
            }
            case "m_isHierarchicalCompound":
            case "isHierarchicalCompound":
            {
                value = instance.m_isHierarchicalCompound;
                return true;
            }
            case "m_hkdShapesCollected":
            case "hkdShapesCollected":
            {
                value = instance.m_hkdShapesCollected;
                return true;
            }
            case "m_childShapeNames":
            case "childShapeNames":
            {
                value = instance.m_childShapeNames;
                return true;
            }
            case "m_childTransforms":
            case "childTransforms":
            {
                value = instance.m_childTransforms;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
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
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_isHierarchicalCompound":
            case "isHierarchicalCompound":
            {
                if (value is not bool castValue) return false;
                instance.m_isHierarchicalCompound = castValue;
                return true;
            }
            case "m_hkdShapesCollected":
            case "hkdShapesCollected":
            {
                if (value is not bool castValue) return false;
                instance.m_hkdShapesCollected = castValue;
                return true;
            }
            case "m_childShapeNames":
            case "childShapeNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_childShapeNames = castValue;
                return true;
            }
            case "m_childTransforms":
            case "childTransforms":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_childTransforms = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
