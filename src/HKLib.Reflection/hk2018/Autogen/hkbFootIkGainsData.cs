// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbFootIkGainsData : HavokData<hkbFootIkGains> 
{
    public hkbFootIkGainsData(HavokType type, hkbFootIkGains instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_onOffGain":
            case "onOffGain":
            {
                if (instance.m_onOffGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_groundAscendingGain":
            case "groundAscendingGain":
            {
                if (instance.m_groundAscendingGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_groundDescendingGain":
            case "groundDescendingGain":
            {
                if (instance.m_groundDescendingGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_footPlantedGain":
            case "footPlantedGain":
            {
                if (instance.m_footPlantedGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_footRaisedGain":
            case "footRaisedGain":
            {
                if (instance.m_footRaisedGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_footLockingGain":
            case "footLockingGain":
            {
                if (instance.m_footLockingGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ankleRotationGain":
            case "ankleRotationGain":
            {
                if (instance.m_ankleRotationGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_worldFromModelFeedbackGain":
            case "worldFromModelFeedbackGain":
            {
                if (instance.m_worldFromModelFeedbackGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_errorUpDownBias":
            case "errorUpDownBias":
            {
                if (instance.m_errorUpDownBias is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_alignWorldFromModelGain":
            case "alignWorldFromModelGain":
            {
                if (instance.m_alignWorldFromModelGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hipOrientationGain":
            case "hipOrientationGain":
            {
                if (instance.m_hipOrientationGain is not TGet castValue) return false;
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
