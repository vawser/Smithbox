// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpStaticCompoundShapeKeyMaskData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpStaticCompoundShapeKeyMask>
{
    public hknpStaticCompoundShapeKeyMaskData(Havok.hk2014.hknpStaticCompoundShapeKeyMask instance) : base(instance) { }

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
            case "m_instanceMasks":
            case "instanceMasks":
            {
                value = instance.m_instanceMasks;
                return true;
            }
            case "m_filter":
            case "filter":
            {
                value = instance.m_filter;
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
                if (value is Havok.hk2014.hknpCompoundShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_instanceMasks":
            case "instanceMasks":
            {
                if (value is not List<Havok.hk2014.hknpShapeKeyMask?> castValue) return false;
                instance.m_instanceMasks = castValue;
                return true;
            }
            case "m_filter":
            case "filter":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_filter = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
