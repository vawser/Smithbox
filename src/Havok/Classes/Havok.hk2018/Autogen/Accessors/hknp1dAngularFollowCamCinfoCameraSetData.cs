// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknp1dAngularFollowCamCinfoCameraSetData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknp1dAngularFollowCamCinfo.CameraSet>
{
    public hknp1dAngularFollowCamCinfoCameraSetData(Havok.hk2018.hknp1dAngularFollowCamCinfo.CameraSet instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_positionUS":
            case "positionUS":
            {
                value = instance.m_positionUS;
                return true;
            }
            case "m_lookAtUS":
            case "lookAtUS":
            {
                value = instance.m_lookAtUS;
                return true;
            }
            case "m_fov":
            case "fov":
            {
                value = instance.m_fov;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                value = instance.m_velocity;
                return true;
            }
            case "m_speedInfluenceOnCameraDirection":
            case "speedInfluenceOnCameraDirection":
            {
                value = instance.m_speedInfluenceOnCameraDirection;
                return true;
            }
            case "m_angularRelaxation":
            case "angularRelaxation":
            {
                value = instance.m_angularRelaxation;
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
