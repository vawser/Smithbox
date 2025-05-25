// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknp1dAngularFollowCamCinfoCameraSetData : HavokData<hknp1dAngularFollowCamCinfo.CameraSet> 
{
    public hknp1dAngularFollowCamCinfoCameraSetData(HavokType type, hknp1dAngularFollowCamCinfo.CameraSet instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_positionUS":
            case "positionUS":
            {
                if (instance.m_positionUS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lookAtUS":
            case "lookAtUS":
            {
                if (instance.m_lookAtUS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fov":
            case "fov":
            {
                if (instance.m_fov is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                if (instance.m_velocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_speedInfluenceOnCameraDirection":
            case "speedInfluenceOnCameraDirection":
            {
                if (instance.m_speedInfluenceOnCameraDirection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angularRelaxation":
            case "angularRelaxation":
            {
                if (instance.m_angularRelaxation is not TGet castValue) return false;
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
            case "m_positionUS":
            case "positionUS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_positionUS = castValue;
                return true;
            }
            case "m_lookAtUS":
            case "lookAtUS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_lookAtUS = castValue;
                return true;
            }
            case "m_fov":
            case "fov":
            {
                if (value is not float castValue) return false;
                instance.m_fov = castValue;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                if (value is not float castValue) return false;
                instance.m_velocity = castValue;
                return true;
            }
            case "m_speedInfluenceOnCameraDirection":
            case "speedInfluenceOnCameraDirection":
            {
                if (value is not float castValue) return false;
                instance.m_speedInfluenceOnCameraDirection = castValue;
                return true;
            }
            case "m_angularRelaxation":
            case "angularRelaxation":
            {
                if (value is not float castValue) return false;
                instance.m_angularRelaxation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
