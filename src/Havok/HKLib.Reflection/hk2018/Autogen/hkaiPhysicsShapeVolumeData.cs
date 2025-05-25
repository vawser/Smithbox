// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiPhysicsShapeVolumeData : HavokData<hkaiPhysicsShapeVolume> 
{
    public hkaiPhysicsShapeVolumeData(HavokType type, hkaiPhysicsShapeVolume instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is hknpShape castValue)
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
                if (value is not hkGeometry castValue) return false;
                instance.m_geometry = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
