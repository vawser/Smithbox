// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiLineOfSightUtilLineOfSightOutputData : HavokData<hkaiLineOfSightUtil.LineOfSightOutput> 
{
    public hkaiLineOfSightUtilLineOfSightOutputData(HavokType type, hkaiLineOfSightUtil.LineOfSightOutput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_visitedEdgesOut":
            case "visitedEdgesOut":
            {
                if (instance.m_visitedEdgesOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_distancesOut":
            case "distancesOut":
            {
                if (instance.m_distancesOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pointsOut":
            case "pointsOut":
            {
                if (instance.m_pointsOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_doNotExceedArrayCapacity":
            case "doNotExceedArrayCapacity":
            {
                if (instance.m_doNotExceedArrayCapacity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numIterationsOut":
            case "numIterationsOut":
            {
                if (instance.m_numIterationsOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_finalFaceKey":
            case "finalFaceKey":
            {
                if (instance.m_finalFaceKey is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_accumulatedDistance":
            case "accumulatedDistance":
            {
                if (instance.m_accumulatedDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_finalPoint":
            case "finalPoint":
            {
                if (instance.m_finalPoint is not TGet castValue) return false;
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
