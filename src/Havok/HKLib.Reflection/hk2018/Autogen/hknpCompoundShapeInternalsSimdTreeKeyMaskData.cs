// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpCompoundShapeInternalsSimdTreeKeyMaskData : HavokData<hknpCompoundShapeInternalsSimdTreeKeyMask> 
{
    public hknpCompoundShapeInternalsSimdTreeKeyMaskData(HavokType type, hknpCompoundShapeInternalsSimdTreeKeyMask instance) : base(type, instance) {}

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
            case "m_instanceMasks":
            case "instanceMasks":
            {
                if (instance.m_instanceMasks is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableInstances":
            case "enableInstances":
            {
                if (instance.m_enableInstances is not TGet castValue) return false;
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
                if (value is hknpCompoundShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_instanceMasks":
            case "instanceMasks":
            {
                if (value is not List<hknpShapeKeyMask?> castValue) return false;
                instance.m_instanceMasks = castValue;
                return true;
            }
            case "m_enableInstances":
            case "enableInstances":
            {
                if (value is not hkBitField castValue) return false;
                instance.m_enableInstances = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
