// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbFootIkModifierLegData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbFootIkModifier.Leg>
{
    public hkbFootIkModifierLegData(Havok.hk2014.hkbFootIkModifier.Leg instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_originalAnkleTransformMS":
            case "originalAnkleTransformMS":
            {
                value = instance.m_originalAnkleTransformMS;
                return true;
            }
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
            case "m_ungroundedEvent":
            case "ungroundedEvent":
            {
                value = instance.m_ungroundedEvent;
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
            case "m_verticalError":
            case "verticalError":
            {
                value = instance.m_verticalError;
                return true;
            }
            case "m_hipIndex":
            case "hipIndex":
            {
                value = instance.m_hipIndex;
                return true;
            }
            case "m_kneeIndex":
            case "kneeIndex":
            {
                value = instance.m_kneeIndex;
                return true;
            }
            case "m_ankleIndex":
            case "ankleIndex":
            {
                value = instance.m_ankleIndex;
                return true;
            }
            case "m_hitSomething":
            case "hitSomething":
            {
                value = instance.m_hitSomething;
                return true;
            }
            case "m_isPlantedMS":
            case "isPlantedMS":
            {
                value = instance.m_isPlantedMS;
                return true;
            }
            case "m_isOriginalAnkleTransformMSSet":
            case "isOriginalAnkleTransformMSSet":
            {
                value = instance.m_isOriginalAnkleTransformMSSet;
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
            case "m_originalAnkleTransformMS":
            case "originalAnkleTransformMS":
            {
                if (value is not Havok.hk2014.hkQsTransform castValue) return false;
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
                if (value is not Havok.hk2014.hkbEventProperty castValue) return false;
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
