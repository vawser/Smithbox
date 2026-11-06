// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpShapeInstanceData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpShapeInstance>
{
    private static readonly System.Reflection.FieldInfo _paddingInfo = typeof(Havok.hk2014.hknpShapeInstance).GetField("m_padding")!;
    public hknpShapeInstanceData(Havok.hk2014.hknpShapeInstance instance) : base(instance) { }

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
            case "m_padding":
            case "padding":
            {
                value = instance.m_padding;
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
                if (value is Havok.hk2014.hknpShape castValue)
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
            case "m_padding":
            case "padding":
            {
                if (value is not byte[] castValue || castValue.Length != 30) return false;
                try
                {
                    _paddingInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
