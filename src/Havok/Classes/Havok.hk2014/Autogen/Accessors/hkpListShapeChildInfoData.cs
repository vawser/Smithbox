// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpListShapeChildInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpListShapeChildInfo>
{
    public hkpListShapeChildInfoData(Havok.hk2014.hkpListShapeChildInfo instance) : base(instance) { }

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
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                value = instance.m_collisionFilterInfo;
                return true;
            }
            case "m_shapeInfo":
            case "shapeInfo":
            {
                value = instance.m_shapeInfo;
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
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_collisionFilterInfo = castValue;
                return true;
            }
            case "m_shapeInfo":
            case "shapeInfo":
            {
                if (value is not ushort castValue) return false;
                instance.m_shapeInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
