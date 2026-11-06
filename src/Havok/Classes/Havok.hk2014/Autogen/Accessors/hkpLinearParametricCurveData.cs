// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpLinearParametricCurveData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpLinearParametricCurve>
{
    public hkpLinearParametricCurveData(Havok.hk2014.hkpLinearParametricCurve instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_smoothingFactor":
            case "smoothingFactor":
            {
                value = instance.m_smoothingFactor;
                return true;
            }
            case "m_closedLoop":
            case "closedLoop":
            {
                value = instance.m_closedLoop;
                return true;
            }
            case "m_dirNotParallelToTangentAlongWholePath":
            case "dirNotParallelToTangentAlongWholePath":
            {
                value = instance.m_dirNotParallelToTangentAlongWholePath;
                return true;
            }
            case "m_points":
            case "points":
            {
                value = instance.m_points;
                return true;
            }
            case "m_distance":
            case "distance":
            {
                value = instance.m_distance;
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
