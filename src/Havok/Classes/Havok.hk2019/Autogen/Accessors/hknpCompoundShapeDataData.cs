// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpCompoundShapeDataData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpCompoundShapeData>
{
    public hknpCompoundShapeDataData(Havok.hk2019.hknpCompoundShapeData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aabbTree":
            case "aabbTree":
            {
                value = instance.m_aabbTree;
                return true;
            }
            case "m_simdTree":
            case "simdTree":
            {
                value = instance.m_simdTree;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
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
            case "m_aabbTree":
            case "aabbTree":
            {
                if (value is not Havok.hk2019.hknpCompoundShapeCdDynamicTree castValue) return false;
                instance.m_aabbTree = castValue;
                return true;
            }
            case "m_simdTree":
            case "simdTree":
            {
                if (value is not Havok.hk2019.hknpCompoundShapeSimdTree castValue) return false;
                instance.m_simdTree = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2019.hknpCompoundShapeBoundingVolumeType.Enum castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2019.hknpCompoundShapeBoundingVolumeType.Enum)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
