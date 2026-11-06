// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiLineOfSightUtilLineOfSightOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiLineOfSightUtil.LineOfSightOutput>
{
    public hkaiLineOfSightUtilLineOfSightOutputData(Havok.hk2019.hkaiLineOfSightUtil.LineOfSightOutput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_visitedEdgesOut":
            case "visitedEdgesOut":
            {
                value = instance.m_visitedEdgesOut;
                return true;
            }
            case "m_distancesOut":
            case "distancesOut":
            {
                value = instance.m_distancesOut;
                return true;
            }
            case "m_pointsOut":
            case "pointsOut":
            {
                value = instance.m_pointsOut;
                return true;
            }
            case "m_doNotExceedArrayCapacity":
            case "doNotExceedArrayCapacity":
            {
                value = instance.m_doNotExceedArrayCapacity;
                return true;
            }
            case "m_numIterationsOut":
            case "numIterationsOut":
            {
                value = instance.m_numIterationsOut;
                return true;
            }
            case "m_finalFaceKey":
            case "finalFaceKey":
            {
                value = instance.m_finalFaceKey;
                return true;
            }
            case "m_accumulatedDistance":
            case "accumulatedDistance":
            {
                value = instance.m_accumulatedDistance;
                return true;
            }
            case "m_finalPoint":
            case "finalPoint":
            {
                value = instance.m_finalPoint;
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
            case "m_visitedEdgesOut":
            case "visitedEdgesOut":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_visitedEdgesOut = castValue;
                return true;
            }
            case "m_distancesOut":
            case "distancesOut":
            {
                if (value is not List<float> castValue) return false;
                instance.m_distancesOut = castValue;
                return true;
            }
            case "m_pointsOut":
            case "pointsOut":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_pointsOut = castValue;
                return true;
            }
            case "m_doNotExceedArrayCapacity":
            case "doNotExceedArrayCapacity":
            {
                if (value is not bool castValue) return false;
                instance.m_doNotExceedArrayCapacity = castValue;
                return true;
            }
            case "m_numIterationsOut":
            case "numIterationsOut":
            {
                if (value is not int castValue) return false;
                instance.m_numIterationsOut = castValue;
                return true;
            }
            case "m_finalFaceKey":
            case "finalFaceKey":
            {
                if (value is not uint castValue) return false;
                instance.m_finalFaceKey = castValue;
                return true;
            }
            case "m_accumulatedDistance":
            case "accumulatedDistance":
            {
                if (value is not float castValue) return false;
                instance.m_accumulatedDistance = castValue;
                return true;
            }
            case "m_finalPoint":
            case "finalPoint":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_finalPoint = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
