// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpCompoundShapeInternalsSimdTreeKeyMaskData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpCompoundShapeInternalsSimdTreeKeyMask>
{
    public hknpCompoundShapeInternalsSimdTreeKeyMaskData(Havok.hk2019.hknpCompoundShapeInternalsSimdTreeKeyMask instance) : base(instance) { }

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
            case "m_enableInstances":
            case "enableInstances":
            {
                value = instance.m_enableInstances;
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
                if (value is Havok.hk2019.hknpCompoundShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_instanceMasks":
            case "instanceMasks":
            {
                if (value is not List<Havok.hk2019.hknpShapeKeyMask?> castValue) return false;
                instance.m_instanceMasks = castValue;
                return true;
            }
            case "m_enableInstances":
            case "enableInstances":
            {
                if (value is not Havok.hk2019.hkBitField castValue) return false;
                instance.m_enableInstances = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
