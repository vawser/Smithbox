// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpLinearParametricCurveData : HavokData<hkpLinearParametricCurve> 
{
    public hkpLinearParametricCurveData(HavokType type, hkpLinearParametricCurve instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_smoothingFactor":
            case "smoothingFactor":
            {
                if (instance.m_smoothingFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_closedLoop":
            case "closedLoop":
            {
                if (instance.m_closedLoop is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dirNotParallelToTangentAlongWholePath":
            case "dirNotParallelToTangentAlongWholePath":
            {
                if (instance.m_dirNotParallelToTangentAlongWholePath is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_points":
            case "points":
            {
                if (instance.m_points is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_distance":
            case "distance":
            {
                if (instance.m_distance is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_smoothingFactor":
            case "smoothingFactor":
            {
                if (value is not float castValue) return false;
                instance.m_smoothingFactor = castValue;
                return true;
            }
            case "m_closedLoop":
            case "closedLoop":
            {
                if (value is not bool castValue) return false;
                instance.m_closedLoop = castValue;
                return true;
            }
            case "m_dirNotParallelToTangentAlongWholePath":
            case "dirNotParallelToTangentAlongWholePath":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_dirNotParallelToTangentAlongWholePath = castValue;
                return true;
            }
            case "m_points":
            case "points":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_points = castValue;
                return true;
            }
            case "m_distance":
            case "distance":
            {
                if (value is not List<float> castValue) return false;
                instance.m_distance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
