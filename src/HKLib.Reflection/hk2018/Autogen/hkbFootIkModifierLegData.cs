// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbFootIkModifierLegData : HavokData<hkbFootIkModifier.Leg> 
{
    public hkbFootIkModifierLegData(HavokType type, hkbFootIkModifier.Leg instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_originalAnkleTransformMS":
            case "originalAnkleTransformMS":
            {
                if (instance.m_originalAnkleTransformMS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_ungroundedEvent":
            case "ungroundedEvent":
            {
                if (instance.m_ungroundedEvent is not TGet castValue) return false;
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
            case "m_verticalError":
            case "verticalError":
            {
                if (instance.m_verticalError is not TGet castValue) return false;
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
            case "m_kneeIndex":
            case "kneeIndex":
            {
                if (instance.m_kneeIndex is not TGet castValue) return false;
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
            case "m_hitSomething":
            case "hitSomething":
            {
                if (instance.m_hitSomething is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isPlantedMS":
            case "isPlantedMS":
            {
                if (instance.m_isPlantedMS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isOriginalAnkleTransformMSSet":
            case "isOriginalAnkleTransformMSSet":
            {
                if (instance.m_isOriginalAnkleTransformMSSet is not TGet castValue) return false;
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
            case "m_originalAnkleTransformMS":
            case "originalAnkleTransformMS":
            {
                if (value is not hkQsTransform castValue) return false;
                instance.m_originalAnkleTransformMS = castValue;
                return true;
            }
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
            case "m_ungroundedEvent":
            case "ungroundedEvent":
            {
                if (value is not hkbEventProperty castValue) return false;
                instance.m_ungroundedEvent = castValue;
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
            case "m_verticalError":
            case "verticalError":
            {
                if (value is not float castValue) return false;
                instance.m_verticalError = castValue;
                return true;
            }
            case "m_hipIndex":
            case "hipIndex":
            {
                if (value is not short castValue) return false;
                instance.m_hipIndex = castValue;
                return true;
            }
            case "m_kneeIndex":
            case "kneeIndex":
            {
                if (value is not short castValue) return false;
                instance.m_kneeIndex = castValue;
                return true;
            }
            case "m_ankleIndex":
            case "ankleIndex":
            {
                if (value is not short castValue) return false;
                instance.m_ankleIndex = castValue;
                return true;
            }
            case "m_hitSomething":
            case "hitSomething":
            {
                if (value is not bool castValue) return false;
                instance.m_hitSomething = castValue;
                return true;
            }
            case "m_isPlantedMS":
            case "isPlantedMS":
            {
                if (value is not bool castValue) return false;
                instance.m_isPlantedMS = castValue;
                return true;
            }
            case "m_isOriginalAnkleTransformMSSet":
            case "isOriginalAnkleTransformMSSet":
            {
                if (value is not bool castValue) return false;
                instance.m_isOriginalAnkleTransformMSSet = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
