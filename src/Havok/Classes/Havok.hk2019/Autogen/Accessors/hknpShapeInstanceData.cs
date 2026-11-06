// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpShapeInstanceData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpShapeInstance>
{
    public hknpShapeInstanceData(Havok.hk2019.hknpShapeInstance instance) : base(instance) { }

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
            case "m_scale":
            case "scale":
            {
                value = instance.m_scale;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                value = instance.m_shape;
                return true;
            }
            case "m_shapeTag":
            case "shapeTag":
            {
                value = instance.m_shapeTag;
                return true;
            }
            case "m_destructionTag":
            case "destructionTag":
            {
                value = instance.m_destructionTag;
                return true;
            }
            case "m_isEmpty":
            case "isEmpty":
            {
                value = instance.m_isEmpty;
                return true;
            }
            case "m_nextEmptyElement":
            case "nextEmptyElement":
            {
                value = instance.m_nextEmptyElement;
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
            case "m_scale":
            case "scale":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_scale = castValue;
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
                if (value is Havok.hk2019.hknpShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_shapeTag":
            case "shapeTag":
            {
                if (value is not ushort castValue) return false;
                instance.m_shapeTag = castValue;
                return true;
            }
            case "m_destructionTag":
            case "destructionTag":
            {
                if (value is not ushort castValue) return false;
                instance.m_destructionTag = castValue;
                return true;
            }
            case "m_isEmpty":
            case "isEmpty":
            {
                if (value is not byte castValue) return false;
                instance.m_isEmpty = castValue;
                return true;
            }
            case "m_nextEmptyElement":
            case "nextEmptyElement":
            {
                if (value is not uint castValue) return false;
                instance.m_nextEmptyElement = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
