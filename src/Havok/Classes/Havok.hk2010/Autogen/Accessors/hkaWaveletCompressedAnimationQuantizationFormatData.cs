// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkaWaveletCompressedAnimationQuantizationFormatData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkaWaveletCompressedAnimationQuantizationFormat>
{
    public hkaWaveletCompressedAnimationQuantizationFormatData(Havok.hk2010.hkaWaveletCompressedAnimationQuantizationFormat instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxBitWidth":
            case "maxBitWidth":
            {
                value = instance.m_maxBitWidth;
                return true;
            }
            case "m_preserved":
            case "preserved":
            {
                value = instance.m_preserved;
                return true;
            }
            case "m_numD":
            case "numD":
            {
                value = instance.m_numD;
                return true;
            }
            case "m_offsetIdx":
            case "offsetIdx":
            {
                value = instance.m_offsetIdx;
                return true;
            }
            case "m_scaleIdx":
            case "scaleIdx":
            {
                value = instance.m_scaleIdx;
                return true;
            }
            case "m_bitWidthIdx":
            case "bitWidthIdx":
            {
                value = instance.m_bitWidthIdx;
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
            case "m_maxBitWidth":
            case "maxBitWidth":
            {
                if (value is not byte castValue) return false;
                instance.m_maxBitWidth = castValue;
                return true;
            }
            case "m_preserved":
            case "preserved":
            {
                if (value is not byte castValue) return false;
                instance.m_preserved = castValue;
                return true;
            }
            case "m_numD":
            case "numD":
            {
                if (value is not uint castValue) return false;
                instance.m_numD = castValue;
                return true;
            }
            case "m_offsetIdx":
            case "offsetIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_offsetIdx = castValue;
                return true;
            }
            case "m_scaleIdx":
            case "scaleIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_scaleIdx = castValue;
                return true;
            }
            case "m_bitWidthIdx":
            case "bitWidthIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_bitWidthIdx = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
