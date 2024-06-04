// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbFootIkDriverInfoLegData : HavokData<hkbFootIkDriverInfo.Leg> 
{
    public hkbFootIkDriverInfoLegData(HavokType type, hkbFootIkDriverInfo.Leg instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_kneeAxisLS":
            case "kneeAxisLS":
            {
                if (instance.m_kneeAxisLS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_footEndLS":
            case "footEndLS":
            {
                if (instance.m_footEndLS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useFootEndToOnlyHeelRay":
            case "useFootEndToOnlyHeelRay":
            {
                if (instance.m_useFootEndToOnlyHeelRay is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useAlwaysContinuousFlag":
            case "useAlwaysContinuousFlag":
            {
                if (instance.m_useAlwaysContinuousFlag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_footPlantedAnkleHeightMS":
            case "footPlantedAnkleHeightMS":
            {
                if (instance.m_footPlantedAnkleHeightMS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_footRaisedAnkleHeightMS":
            case "footRaisedAnkleHeightMS":
            {
                if (instance.m_footRaisedAnkleHeightMS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxAnkleHeightMS":
            case "maxAnkleHeightMS":
            {
                if (instance.m_maxAnkleHeightMS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minAnkleHeightMS":
            case "minAnkleHeightMS":
            {
                if (instance.m_minAnkleHeightMS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxFootPitchDegrees":
            case "maxFootPitchDegrees":
            {
                if (instance.m_maxFootPitchDegrees is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minFootPitchDegrees":
            case "minFootPitchDegrees":
            {
                if (instance.m_minFootPitchDegrees is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxFootRollDegrees":
            case "maxFootRollDegrees":
            {
                if (instance.m_maxFootRollDegrees is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minFootRollDegrees":
            case "minFootRollDegrees":
            {
                if (instance.m_minFootRollDegrees is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_heelOffsetFromAnkle":
            case "heelOffsetFromAnkle":
            {
                if (instance.m_heelOffsetFromAnkle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_favorToeInterpenetrationOverSteepSlope":
            case "favorToeInterpenetrationOverSteepSlope":
            {
                if (instance.m_favorToeInterpenetrationOverSteepSlope is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_favorHeelInterpenetrationOverSteepSlope":
            case "favorHeelInterpenetrationOverSteepSlope":
            {
                if (instance.m_favorHeelInterpenetrationOverSteepSlope is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxKneeAngleDegrees":
            case "maxKneeAngleDegrees":
            {
                if (instance.m_maxKneeAngleDegrees is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minKneeAngleDegrees":
            case "minKneeAngleDegrees":
            {
                if (instance.m_minKneeAngleDegrees is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hipIndex":
            case "hipIndex":
            {
                if (instance.m_hipIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hipSiblingIndex":
            case "hipSiblingIndex":
            {
                if (instance.m_hipSiblingIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_kneeIndex":
            case "kneeIndex":
            {
                if (instance.m_kneeIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_kneeSiblingIndex":
            case "kneeSiblingIndex":
            {
                if (instance.m_kneeSiblingIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ankleIndex":
            case "ankleIndex":
            {
                if (instance.m_ankleIndex is not TGet castValue) return false;
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
