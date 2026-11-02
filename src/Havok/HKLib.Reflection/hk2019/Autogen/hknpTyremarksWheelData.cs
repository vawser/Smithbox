// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpTyremarksWheelData : HavokData<HKLib.hk2019.hknpTyremarksWheel> 
{
    public hknpTyremarksWheelData(HavokType type, HKLib.hk2019.hknpTyremarksWheel instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_currentPosition":
            case "currentPosition":
            {
                if (instance.m_currentPosition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numPoints":
            case "numPoints":
            {
                if (instance.m_numPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tyremarkPoints":
            case "tyremarkPoints":
            {
                if (instance.m_tyremarkPoints is not TGet castValue) return false;
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
            case "m_currentPosition":
            case "currentPosition":
            {
                if (value is not int castValue) return false;
                instance.m_currentPosition = castValue;
                return true;
            }
            case "m_numPoints":
            case "numPoints":
            {
                if (value is not int castValue) return false;
                instance.m_numPoints = castValue;
                return true;
            }
            case "m_tyremarkPoints":
            case "tyremarkPoints":
            {
                if (value is not List<HKLib.hk2019.hknpTyremarkPoint> castValue) return false;
                instance.m_tyremarkPoints = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
