// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpCompoundShapeDataData : HavokData<HKLib.hk2019.hknpCompoundShapeData> 
{
    public hknpCompoundShapeDataData(HavokType type, HKLib.hk2019.hknpCompoundShapeData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aabbTree":
            case "aabbTree":
            {
                if (instance.m_aabbTree is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simdTree":
            case "simdTree":
            {
                if (instance.m_simdTree is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_type is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_aabbTree":
            case "aabbTree":
            {
                if (value is not HKLib.hk2019.hknpCompoundShapeCdDynamicTree castValue) return false;
                instance.m_aabbTree = castValue;
                return true;
            }
            case "m_simdTree":
            case "simdTree":
            {
                if (value is not HKLib.hk2019.hknpCompoundShapeSimdTree castValue) return false;
                instance.m_simdTree = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is HKLib.hk2019.hknpCompoundShapeBoundingVolumeType.Enum castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (HKLib.hk2019.hknpCompoundShapeBoundingVolumeType.Enum)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
