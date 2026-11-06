// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbFootIkDriverInfoLegData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbFootIkDriverInfo.Leg>
{
    public hkbFootIkDriverInfoLegData(Havok.hk2018.hkbFootIkDriverInfo.Leg instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_kneeAxisLS":
            case "kneeAxisLS":
            {
                value = instance.m_kneeAxisLS;
                return true;
            }
            case "m_footEndLS":
            case "footEndLS":
            {
                value = instance.m_footEndLS;
                return true;
            }
            case "m_useFootEndToOnlyHeelRay":
            case "useFootEndToOnlyHeelRay":
            {
                value = instance.m_useFootEndToOnlyHeelRay;
                return true;
            }
            case "m_useAlwaysContinuousFlag":
            case "useAlwaysContinuousFlag":
            {
                value = instance.m_useAlwaysContinuousFlag;
                return true;
            }
            case "m_footPlantedAnkleHeightMS":
            case "footPlantedAnkleHeightMS":
            {
                value = instance.m_footPlantedAnkleHeightMS;
                return true;
            }
            case "m_footRaisedAnkleHeightMS":
            case "footRaisedAnkleHeightMS":
            {
                value = instance.m_footRaisedAnkleHeightMS;
                return true;
            }
            case "m_maxAnkleHeightMS":
            case "maxAnkleHeightMS":
            {
                value = instance.m_maxAnkleHeightMS;
                return true;
            }
            case "m_minAnkleHeightMS":
            case "minAnkleHeightMS":
            {
                value = instance.m_minAnkleHeightMS;
                return true;
            }
            case "m_maxFootPitchDegrees":
            case "maxFootPitchDegrees":
            {
                value = instance.m_maxFootPitchDegrees;
                return true;
            }
            case "m_minFootPitchDegrees":
            case "minFootPitchDegrees":
            {
                value = instance.m_minFootPitchDegrees;
                return true;
            }
            case "m_maxFootRollDegrees":
            case "maxFootRollDegrees":
            {
                value = instance.m_maxFootRollDegrees;
                return true;
            }
            case "m_minFootRollDegrees":
            case "minFootRollDegrees":
            {
                value = instance.m_minFootRollDegrees;
                return true;
            }
            case "m_heelOffsetFromAnkle":
            case "heelOffsetFromAnkle":
            {
                value = instance.m_heelOffsetFromAnkle;
                return true;
            }
            case "m_favorToeInterpenetrationOverSteepSlope":
            case "favorToeInterpenetrationOverSteepSlope":
            {
                value = instance.m_favorToeInterpenetrationOverSteepSlope;
                return true;
            }
            case "m_favorHeelInterpenetrationOverSteepSlope":
            case "favorHeelInterpenetrationOverSteepSlope":
            {
                value = instance.m_favorHeelInterpenetrationOverSteepSlope;
                return true;
            }
            case "m_maxKneeAngleDegrees":
            case "maxKneeAngleDegrees":
            {
                value = instance.m_maxKneeAngleDegrees;
                return true;
            }
            case "m_minKneeAngleDegrees":
            case "minKneeAngleDegrees":
            {
                value = instance.m_minKneeAngleDegrees;
                return true;
            }
            case "m_hipIndex":
            case "hipIndex":
            {
                value = instance.m_hipIndex;
                return true;
            }
            case "m_hipSiblingIndex":
            case "hipSiblingIndex":
            {
                value = instance.m_hipSiblingIndex;
                return true;
            }
            case "m_kneeIndex":
            case "kneeIndex":
            {
                value = instance.m_kneeIndex;
                return true;
            }
            case "m_kneeSiblingIndex":
            case "kneeSiblingIndex":
            {
                value = instance.m_kneeSiblingIndex;
                return true;
            }
            case "m_ankleIndex":
            case "ankleIndex":
            {
                value = instance.m_ankleIndex;
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
            case "m_kneeAxisLS":
            case "kneeAxisLS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_kneeAxisLS = castValue;
                return true;
            }
            case "m_footEndLS":
            case "footEndLS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_footEndLS = castValue;
                return true;
            }
            case "m_useFootEndToOnlyHeelRay":
            case "useFootEndToOnlyHeelRay":
            {
                if (value is not bool castValue) return false;
                instance.m_useFootEndToOnlyHeelRay = castValue;
                return true;
            }
            case "m_useAlwaysContinuousFlag":
            case "useAlwaysContinuousFlag":
            {
                if (value is not bool castValue) return false;
                instance.m_useAlwaysContinuousFlag = castValue;
                return true;
            }
            case "m_footPlantedAnkleHeightMS":
            case "footPlantedAnkleHeightMS":
            {
                if (value is not float castValue) return false;
                instance.m_footPlantedAnkleHeightMS = castValue;
                return true;
            }
            case "m_footRaisedAnkleHeightMS":
            case "footRaisedAnkleHeightMS":
            {
                if (value is not float castValue) return false;
                instance.m_footRaisedAnkleHeightMS = castValue;
                return true;
            }
            case "m_maxAnkleHeightMS":
            case "maxAnkleHeightMS":
            {
                if (value is not float castValue) return false;
                instance.m_maxAnkleHeightMS = castValue;
                return true;
            }
            case "m_minAnkleHeightMS":
            case "minAnkleHeightMS":
            {
                if (value is not float castValue) return false;
                instance.m_minAnkleHeightMS = castValue;
                return true;
            }
            case "m_maxFootPitchDegrees":
            case "maxFootPitchDegrees":
            {
                if (value is not float castValue) return false;
                instance.m_maxFootPitchDegrees = castValue;
                return true;
            }
            case "m_minFootPitchDegrees":
            case "minFootPitchDegrees":
            {
                if (value is not float castValue) return false;
                instance.m_minFootPitchDegrees = castValue;
                return true;
            }
            case "m_maxFootRollDegrees":
            case "maxFootRollDegrees":
            {
                if (value is not float castValue) return false;
                instance.m_maxFootRollDegrees = castValue;
                return true;
            }
            case "m_minFootRollDegrees":
            case "minFootRollDegrees":
            {
                if (value is not float castValue) return false;
                instance.m_minFootRollDegrees = castValue;
                return true;
            }
            case "m_heelOffsetFromAnkle":
            case "heelOffsetFromAnkle":
            {
                if (value is not float castValue) return false;
                instance.m_heelOffsetFromAnkle = castValue;
                return true;
            }
            case "m_favorToeInterpenetrationOverSteepSlope":
            case "favorToeInterpenetrationOverSteepSlope":
            {
                if (value is not bool castValue) return false;
                instance.m_favorToeInterpenetrationOverSteepSlope = castValue;
                return true;
            }
            case "m_favorHeelInterpenetrationOverSteepSlope":
            case "favorHeelInterpenetrationOverSteepSlope":
            {
                if (value is not bool castValue) return false;
                instance.m_favorHeelInterpenetrationOverSteepSlope = castValue;
                return true;
            }
            case "m_maxKneeAngleDegrees":
            case "maxKneeAngleDegrees":
            {
                if (value is not float castValue) return false;
                instance.m_maxKneeAngleDegrees = castValue;
                return true;
            }
            case "m_minKneeAngleDegrees":
            case "minKneeAngleDegrees":
            {
                if (value is not float castValue) return false;
                instance.m_minKneeAngleDegrees = castValue;
                return true;
            }
            case "m_hipIndex":
            case "hipIndex":
            {
                if (value is not short castValue) return false;
                instance.m_hipIndex = castValue;
                return true;
            }
            case "m_hipSiblingIndex":
            case "hipSiblingIndex":
            {
                if (value is not short castValue) return false;
                instance.m_hipSiblingIndex = castValue;
                return true;
            }
            case "m_kneeIndex":
            case "kneeIndex":
            {
                if (value is not short castValue) return false;
                instance.m_kneeIndex = castValue;
                return true;
            }
            case "m_kneeSiblingIndex":
            case "kneeSiblingIndex":
            {
                if (value is not short castValue) return false;
                instance.m_kneeSiblingIndex = castValue;
                return true;
            }
            case "m_ankleIndex":
            case "ankleIndex":
            {
                if (value is not short castValue) return false;
                instance.m_ankleIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
