// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiPhysicsShapeVolumeData : HavokData<HKLib.hk2019.hkaiPhysicsShapeVolume> 
{
    public hkaiPhysicsShapeVolumeData(HavokType type, HKLib.hk2019.hkaiPhysicsShapeVolume instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_shape":
            case "shape":
            {
                if (instance.m_shape is null)
                {
                    return true;
                }
                if (instance.m_shape is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_shapeTransform":
            case "shapeTransform":
            {
                if (instance.m_shapeTransform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_geometry":
            case "geometry":
            {
                if (instance.m_geometry is not TGet castValue) return false;
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
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is HKLib.hk2019.hknpShape castValue)
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
                if (value is not HKLib.hk2019.hkGeometry castValue) return false;
                instance.m_geometry = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
