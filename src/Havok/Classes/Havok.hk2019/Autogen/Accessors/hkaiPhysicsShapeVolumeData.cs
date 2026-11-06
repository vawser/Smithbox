// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiPhysicsShapeVolumeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiPhysicsShapeVolume>
{
    public hkaiPhysicsShapeVolumeData(Havok.hk2019.hkaiPhysicsShapeVolume instance) : base(instance) { }

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
            case "m_shapeTransform":
            case "shapeTransform":
            {
                value = instance.m_shapeTransform;
                return true;
            }
            case "m_geometry":
            case "geometry":
            {
                value = instance.m_geometry;
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
                if (value is Havok.hk2019.hknpShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_shapeTransform":
            case "shapeTransform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_shapeTransform = castValue;
                return true;
            }
            case "m_geometry":
            case "geometry":
            {
                if (value is not Havok.hk2019.hkGeometry castValue) return false;
                instance.m_geometry = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
