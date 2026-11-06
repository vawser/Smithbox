// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbFootIkGainsData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbFootIkGains>
{
    public hkbFootIkGainsData(Havok.hk2016.hkbFootIkGains instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_onOffGain":
            case "onOffGain":
            {
                value = instance.m_onOffGain;
                return true;
            }
            case "m_groundAscendingGain":
            case "groundAscendingGain":
            {
                value = instance.m_groundAscendingGain;
                return true;
            }
            case "m_groundDescendingGain":
            case "groundDescendingGain":
            {
                value = instance.m_groundDescendingGain;
                return true;
            }
            case "m_footPlantedGain":
            case "footPlantedGain":
            {
                value = instance.m_footPlantedGain;
                return true;
            }
            case "m_footRaisedGain":
            case "footRaisedGain":
            {
                value = instance.m_footRaisedGain;
                return true;
            }
            case "m_footLockingGain":
            case "footLockingGain":
            {
                value = instance.m_footLockingGain;
                return true;
            }
            case "m_ankleRotationGain":
            case "ankleRotationGain":
            {
                value = instance.m_ankleRotationGain;
                return true;
            }
            case "m_worldFromModelFeedbackGain":
            case "worldFromModelFeedbackGain":
            {
                value = instance.m_worldFromModelFeedbackGain;
                return true;
            }
            case "m_errorUpDownBias":
            case "errorUpDownBias":
            {
                value = instance.m_errorUpDownBias;
                return true;
            }
            case "m_alignWorldFromModelGain":
            case "alignWorldFromModelGain":
            {
                value = instance.m_alignWorldFromModelGain;
                return true;
            }
            case "m_hipOrientationGain":
            case "hipOrientationGain":
            {
                value = instance.m_hipOrientationGain;
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
            case "m_onOffGain":
            case "onOffGain":
            {
                if (value is not float castValue) return false;
                instance.m_onOffGain = castValue;
                return true;
            }
            case "m_groundAscendingGain":
            case "groundAscendingGain":
            {
                if (value is not float castValue) return false;
                instance.m_groundAscendingGain = castValue;
                return true;
            }
            case "m_groundDescendingGain":
            case "groundDescendingGain":
            {
                if (value is not float castValue) return false;
                instance.m_groundDescendingGain = castValue;
                return true;
            }
            case "m_footPlantedGain":
            case "footPlantedGain":
            {
                if (value is not float castValue) return false;
                instance.m_footPlantedGain = castValue;
                return true;
            }
            case "m_footRaisedGain":
            case "footRaisedGain":
            {
                if (value is not float castValue) return false;
                instance.m_footRaisedGain = castValue;
                return true;
            }
            case "m_footLockingGain":
            case "footLockingGain":
            {
                if (value is not float castValue) return false;
                instance.m_footLockingGain = castValue;
                return true;
            }
            case "m_ankleRotationGain":
            case "ankleRotationGain":
            {
                if (value is not float castValue) return false;
                instance.m_ankleRotationGain = castValue;
                return true;
            }
            case "m_worldFromModelFeedbackGain":
            case "worldFromModelFeedbackGain":
            {
                if (value is not float castValue) return false;
                instance.m_worldFromModelFeedbackGain = castValue;
                return true;
            }
            case "m_errorUpDownBias":
            case "errorUpDownBias":
            {
                if (value is not float castValue) return false;
                instance.m_errorUpDownBias = castValue;
                return true;
            }
            case "m_alignWorldFromModelGain":
            case "alignWorldFromModelGain":
            {
                if (value is not float castValue) return false;
                instance.m_alignWorldFromModelGain = castValue;
                return true;
            }
            case "m_hipOrientationGain":
            case "hipOrientationGain":
            {
                if (value is not float castValue) return false;
                instance.m_hipOrientationGain = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
