// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiPathData : HavokData<HKLib.hk2019.hkaiPath> 
{
    public hkaiPathData(HavokType type, HKLib.hk2019.hkaiPath instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_points":
            case "points":
            {
                if (instance.m_points is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (instance.m_referenceFrame is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_referenceFrame is TGet byteValue)
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
            case "m_points":
            case "points":
            {
                if (value is not List<HKLib.hk2019.hkaiPath.PathPoint> castValue) return false;
                instance.m_points = castValue;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (value is HKLib.hk2019.hkaiPath.ReferenceFrame castValue)
                {
                    instance.m_referenceFrame = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_referenceFrame = (HKLib.hk2019.hkaiPath.ReferenceFrame)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
